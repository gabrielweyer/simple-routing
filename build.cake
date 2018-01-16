#tool "nuget:?package=GitVersion.CommandLine&version=3.6.5"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var assemblyVersion = "0.0.1";
var packageVersion = "0.0.1";

var artifactsDir = MakeAbsolute(Directory("artifacts"));
var testsResultsDir = artifactsDir.Combine("test-results");
var packagesDir = artifactsDir.Combine(Directory("packages"));

var solutionPath = "./SimpleRouting.sln";

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(artifactsDir);

        var settings = new DotNetCoreCleanSettings
        {
            Configuration = configuration
        };

        DotNetCoreClean(solutionPath, settings);
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreRestore();
    });

Task("SemVer")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        var gitVersion = GitVersion();

        assemblyVersion = gitVersion.AssemblySemVer;
        packageVersion = gitVersion.NuGetVersion;

        Information($"AssemblySemVer: {assemblyVersion}");
        Information($"NuGetVersion: {packageVersion}");
    });

Task("SetAppVeyorVersion")
    .IsDependentOn("Semver")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
    {
        AppVeyor.UpdateBuildVersion(packageVersion);
    });

Task("Build")
    .IsDependentOn("SetAppVeyorVersion")
    .Does(() =>
    {
        var settings = new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            NoIncremental = true,
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .SetVersion(assemblyVersion)
                .WithProperty("FileVersion", packageVersion)
                .WithProperty("InformationalVersion", packageVersion)
                .WithProperty("nowarn", "7035"),
            ArgumentCustomization = args => args.Append("--no-restore")
        };

        DotNetCoreBuild(solutionPath, settings);
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var settings = new DotNetCoreToolSettings();

        var argumentsBuilder = new ProcessArgumentBuilder()
            .Append("-configuration")
            .Append(configuration)
            .Append("-nobuild");

        var projectFiles = GetFiles("./tests/*/*.Tests.csproj");

        foreach (var projectFile in projectFiles)
        {
            var testResultsFile = testsResultsDir.Combine($"{projectFile.GetFilenameWithoutExtension()}.xml");
            var arguments = $"{argumentsBuilder.Render()} -xml \"{testResultsFile}\"";

            DotNetCoreTool(projectFile, "xunit", arguments, settings);
        }
    });

Task("Pack")
    .IsDependentOn("Test")
    .WithCriteria(() => HasArgument("pack"))
    .Does(() =>
    {
        var settings = new DotNetCorePackSettings
        {
            Configuration = configuration,
            NoBuild = true,
            IncludeSymbols = true,
            OutputDirectory = packagesDir,
            ArgumentCustomization = args => args.Append("--no-restore"),
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .WithProperty("PackageVersion", packageVersion)
        };

        GetFiles("./src/*/*.csproj")
            .ToList()
            .ForEach(f => DotNetCorePack(f.FullPath, settings));
    });

Task("PublishAppVeyor")
    .IsDependentOn("Pack")
    .WithCriteria(() => HasArgument("pack") && AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
    {
        CopyFiles($"{packagesDir}/*.nupkg", MakeAbsolute(Directory("./")), false);

        GetFiles($"./*.nupkg")
            .ToList()
            .ForEach(f => AppVeyor.UploadArtifact(f, new AppVeyorUploadArtifactsSettings { DeploymentName = "packages" }));
    });

Task("Default")
    .IsDependentOn("PublishAppVeyor");

RunTarget(target);

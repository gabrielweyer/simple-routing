using System;
using Amazon.SQS.Model;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace BeanstalkWorker.SimpleRouting.SampleWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args)
        {
            var webHostBuilder = WebHost.CreateDefaultBuilder(args);

            var contentRoot = webHostBuilder.GetSetting("contentRoot");
            var environment = webHostBuilder.GetSetting("ENVIRONMENT");

            var isDevelopment = EnvironmentName.Development.Equals(environment);

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddJsonFile("appsettings.json", false, false)
                .AddEnvironmentVariables();

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets<Program>();
            }

            var configuration = configurationBuilder.Build();

            var serilogLevel = configuration.GetLoggingLevel("MinimumLevel:Default");

            var loggerConfiguration = new LoggerConfiguration()
                .Destructure.ByTransforming<MessageAttributeValue>(a => new {a.DataType, a.StringValue})
                .Enrich.WithDemystifiedStackTraces()
                .ReadFrom.Configuration(configuration);

            if (isDevelopment)
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Console(serilogLevel);
            }

            var logger = loggerConfiguration.CreateLogger();

            try
            {
                logger.Information("Starting Host...");

                SetAwsEnvironmentVariables(configuration, logger);

                return webHostBuilder
                    .UseStartup<Startup>()
                    .UseConfiguration(configuration)
                    .UseSerilog(logger, true)
                    .Build();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Host terminated unexpectedly");
                throw;
            }
        }

        private static void SetAwsEnvironmentVariables(IConfigurationRoot configuration, Logger logger)
        {
            const string awsAccessKeyIdVariableName = "AWS_ACCESS_KEY_ID";
            const string awsSecretAccessKeyVariableName = "AWS_SECRET_ACCESS_KEY";

            var accessKeyIdFromEnvironmentVariable = Environment.GetEnvironmentVariable(awsAccessKeyIdVariableName);
            var secretAccessKeyFromEnvironmentVariable = Environment.GetEnvironmentVariable(awsSecretAccessKeyVariableName);

            if (!string.IsNullOrWhiteSpace(accessKeyIdFromEnvironmentVariable) &&
                !string.IsNullOrWhiteSpace(secretAccessKeyFromEnvironmentVariable))
            {
                logger.Information($"'{awsAccessKeyIdVariableName}' and '{awsSecretAccessKeyVariableName}' set via environment variables");

                return;
            }

            var accessKeyIdFromConfiguration = configuration.GetValue<string>(awsAccessKeyIdVariableName);
            var secretAccessKeyFromConfiguration = configuration.GetValue<string>(awsSecretAccessKeyVariableName);

            if (!string.IsNullOrWhiteSpace(accessKeyIdFromConfiguration) &&
                !string.IsNullOrWhiteSpace(secretAccessKeyFromConfiguration))
            {
                logger.Information($"'{awsAccessKeyIdVariableName}' and '{awsSecretAccessKeyVariableName}' present in configuration, setting matching environment variables");

                Environment.SetEnvironmentVariable(
                    awsAccessKeyIdVariableName,
                    accessKeyIdFromConfiguration,
                    EnvironmentVariableTarget.Process);

                Environment.SetEnvironmentVariable(
                    awsSecretAccessKeyVariableName,
                    secretAccessKeyFromConfiguration,
                    EnvironmentVariableTarget.Process);
            }
            else
            {
                throw new InvalidOperationException($"'{awsAccessKeyIdVariableName}' and '{awsSecretAccessKeyVariableName}' should either be set as environment variables or configuration, please refer to the README: https://github.com/gabrielweyer/simple-routing/blob/master/README.md#configuration.");
            }
        }
    }

    internal static class ConfigurationRootExtensions
    {
        internal static LogEventLevel GetLoggingLevel(this IConfigurationRoot configuration, string keyName,
            LogEventLevel defaultLevel = LogEventLevel.Warning)
        {
            try
            {
                return configuration.GetValue($"Serilog:{keyName}", LogEventLevel.Warning);
            }
            catch (Exception)
            {
                return defaultLevel;
            }
        }
    }
}

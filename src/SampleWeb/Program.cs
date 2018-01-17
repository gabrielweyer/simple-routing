﻿using System;
using Amazon.SQS.Model;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
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

            var configuration = new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddJsonFile("appsettings.json", false, false)
                .AddEnvironmentVariables()
                .Build();

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

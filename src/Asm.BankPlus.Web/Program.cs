using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.AspNetCore;
using Serilog.Sinks.File;
using Microsoft.Extensions.Hosting;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.IdentityModel.Tokens;

namespace Asm.BankPlus.Web
{
    public class Program
    {
        private static readonly string Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        public static int Main(string[] args)
        {
            //Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\..\..\..";

            Log.Logger = ConfigureLogging(new LoggerConfiguration()).CreateLogger();

            try
            {
                Log.Information("Starting...");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal host exception");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .UseSerilog((_, configuration) => ConfigureLogging(configuration))
                    .UseStartup<Startup>());


        private static LoggerConfiguration ConfigureLogging(LoggerConfiguration configuration)
        {
            configuration
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .MinimumLevel.Is(LogEventLevel.Information)
                .WriteTo.Trace()
                .WriteTo.ApplicationInsights(TelemetryConfiguration.CreateDefault(), TelemetryConverter.Traces);

            if (Env == "Development")
            {
                configuration.WriteTo.File(@"logs\Log.log", rollingInterval: RollingInterval.Day);
            }

            return configuration;
        }
    }

}

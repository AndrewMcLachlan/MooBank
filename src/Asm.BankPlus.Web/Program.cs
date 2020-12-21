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

namespace Asm.BankPlus.Web
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\..\..\..";

            //var seq = Configuration.GetValue<string>("Seq:Endpoint");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Information)
                //.MinimumLevel.Override("LOR.DigitalReports", new LoggingLevelSwitch(LogEventLevel.Information))
                //.Enrich.WithProperty("ApplicationEnvironment", appEnv)
                //.Enrich.WithProperty("ApplicationName", appName)
                //.Enrich.WithProperty("ApplicationRole", appRole)
                //.WriteTo.Trace()
                //.WriteTo.Seq(seq)
                //.WriteTo.File("Log.log")
                .CreateLogger();

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
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.UseSerilog((_, configuration) =>
                    {
                        configuration
                        .Enrich.FromLogContext()
                        .MinimumLevel.Information()
                        .MinimumLevel.Is(LogEventLevel.Information)
                        .WriteTo.File("Log.log", rollingInterval: RollingInterval.Day);
                    })
                .UseStartup<Startup>());
    }
}

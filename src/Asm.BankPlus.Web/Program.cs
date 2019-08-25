using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.AspNetCore;
using Serilog.Sinks.File;

namespace Asm.BankPlus.Web
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\..\..\..";

            //var appEnv = Configuration.GetValue<string>("ApplicationEnvironment");
            //var appName = Configuration.GetValue<string>("ApplicationName");
            //var appRole = Configuration.GetValue<string>("ApplicationRole");
            //var seq = Configuration.GetValue<string>("SeqEndpoint");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Information)
                //.MinimumLevel.Override("LOR.DigitalReports", new LoggingLevelSwitch(LogEventLevel.Information))
                //.Enrich.WithProperty("ApplicationEnvironment", appEnv)
                //.Enrich.WithProperty("ApplicationName", appName)
                //.Enrich.WithProperty("ApplicationRole", appRole)
                //.WriteTo.Trace()
                //.WriteTo.Seq(seq)
                .WriteTo.File("Log.log")
                .CreateLogger();

            try
            {
                Log.Information("Starting...");
                CreateWebHostBuilder(args).Build().Run();
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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}

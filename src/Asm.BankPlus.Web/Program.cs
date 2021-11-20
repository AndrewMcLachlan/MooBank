using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Events;

namespace Asm.BankPlus.Web;

public class Program
{
    private static readonly string Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    public static int Main(string[] args)
    {
        //Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\..\..\..";

        Log.Logger = ConfigureLogging(new LoggerConfiguration()).CreateBootstrapLogger();

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
                .UseSerilog((context, configuration) => ConfigureLogging(configuration, context))
                .UseStartup<Startup>());

    private static LoggerConfiguration ConfigureLogging(LoggerConfiguration configuration, WebHostBuilderContext context = null)
    {
        configuration
            .Enrich.FromLogContext()
            .Enrich.WithProperty("App", "MooBank")
            .MinimumLevel.Information()
            .MinimumLevel.Is(LogEventLevel.Information)
            .WriteTo.Trace()
            .WriteTo.ApplicationInsights(TelemetryConfiguration.CreateDefault(), TelemetryConverter.Traces);

        if (context != null)
        {
            IConfigurationSection logLevelConfig = context.Configuration.GetSection("Logging").GetSection("LogLevel");
            configuration
                .MinimumLevel.Override("System", logLevelConfig.GetValue<LogEventLevel>("System"))
                .MinimumLevel.Override("Microsoft", logLevelConfig.GetValue<LogEventLevel>("Microsoft"))
                .WriteTo.Seq(context.Configuration["Seq:Host"], apiKey: context.Configuration["Seq:APIKey"]);
        }

        if (Env == "Development")
        {
            configuration.WriteTo.File(@"logs\Log.log", rollingInterval: RollingInterval.Day);
        }

        return configuration;
    }
}

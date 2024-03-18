using Asm.MooBank.Models;
using Asm.MooBank.Services;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

return await WebJobStart.RunAsync(args, "Asm.MooBank.Web.Jobs", ConfigureWebJobs, ConfigureServices);

static void ConfigureWebJobs(IWebJobsBuilder builder)
{
    builder.AddTimers();
}

static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddMooBankDbContext(context.Configuration);
    services.AddRepositories();
    services.AddEntities();
    services.AddImporterFactory();
    services.AddServices();
    services.AddEodhd(options => context.Configuration.Bind("EODHD", options));
    services.AddExchangeRateApi(options => context.Configuration.Bind("ExchangeRateApi", options));
    services.AddSingleton(new AccountHolder() { EmailAddress = "moobank@mclachlan.family", Currency = String.Empty });

    services.AddSingleton<IAuthorizationService, AuthorisationService>();
    services.AddSingleton<IHttpContextAccessor, DummyHttpContextAccessor>();
    services.AddSingleton<IPrincipalProvider, PrincipalProvider>();

    services.AddScoped<IStockPriceService, StockPriceService>();
    services.AddScoped<IExchangeRateService, ExchangeRateService>();
}

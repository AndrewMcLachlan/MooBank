using System;
using System.IO;
using System.Net;
using Asm.BankPlus.Importers;
using Asm.BankPlus.Repository;
using Asm.BankPlus.Repository.Ing;
using Asm.BankPlus.Security;
using Asm.BankPlus.Services;
using Asm.BankPlus.Services.Importers;
using Asm.BankPlus.Services.Ing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;

namespace Asm.BankPlus.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();

            services.AddControllers();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.AddManagedServiceIdentityForSqlServer();

            services.AddDbContext<Data.BankPlusContext>((services, options) => options.UseSqlServer(Configuration.GetConnectionString("BankPlus"), options =>
            {
                options.EnableRetryOnFailure(3);
            }).AddManagedServiceIdentity(services));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
            });

            services.AddPrincipalProvider();

            AddAuthentication(services);

            services.AddAuthorization();

            RegisterServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)//, ILogger logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.ContentType = "application/json";

                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(CreateProblemDetails(env, context, exceptionHandlerFeature.Error)));
                });
            });


            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
                {
                    OnPrepareResponse = context =>
                    {
                        // never cache index.html
                        if (context.File.Name == "index.html")
                        {
                            context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                            context.Context.Response.Headers.Add("Expires", "-1");
                        }
                    }
                };

                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
                }
            });
        }

        private static void AddAuthentication(IServiceCollection services)
        {
            //services.AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme).AddAzureADBearer((options) => Configuration.Bind("AzureAD", options));

            IdentityModelEventSource.ShowPII = true;
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddAzureAdBearer(options =>
            {
                options.TenantId = "30efefb9-9034-4e0c-8c69-17f4578f5924";
                options.ADAuthority = "https://login.microsoftonline.com/";
                options.Audience = "045f8afa-70f2-4700-ab75-77ac41b306f7";
                options.ApplicationId = "045f8afa-70f2-4700-ab75-77ac41b306f7";
                options.ApplicationSecret = "gvwLXheN2Ba2OKFE*AxKi9jupyNq6.]+";
                options.CallbackPath = "/signin-oidc";
                options.RedirectUrl = null;
            });
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ITransactionTagRepository, TransactionTagRepository>();
            services.AddScoped<ITransactionTagRuleRepository, TransactionTagRuleRepository>();
            services.AddScoped<IngImporter>();
            services.AddScoped<IImporterFactory, ImporterFactory>();
            services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();
            services.AddScoped<IAccountHolderRepository, AccountHolderRepository>();
            services.AddScoped<IUserDataProvider, GraphUserDataProvider>();
            services.AddScoped<ISecurityRepository, SecurityRepository>();
            services.AddScoped<ITransactionExtraRepository, TransactionExtraRepository>();
            services.AddScoped<IAccountService, AccountService>();

            services.AddHostedService<RunRulesService>();
            services.AddSingleton<IRunRulesQueue, RunRulesQueue>();
        }

        private static ProblemDetails CreateProblemDetails(IHostEnvironment env, HttpContext context, Exception ex)
        {
            ProblemDetails result = new ProblemDetails();

            switch (ex)
            {
                case NotFoundException _:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    result.Title = "Not found";
                    result.Detail = ex.Message;
                    break;
                case ExistsException _:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    result.Title = "Already exists";
                    result.Detail = ex.Message;
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    result.Title = "Unexpected error occured";
                    result.Detail = !env.IsProduction() ? ex.ToString() : null;
                    break;
            }

            result.Status = context.Response.StatusCode;

            return result;
        }
    }
}

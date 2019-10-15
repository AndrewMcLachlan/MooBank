using System;
using System.IO;
using System.Net;
using Asm.BankPlus.Importers;
using Asm.BankPlus.Repository;
using Asm.BankPlus.Services;
using Asm.BankPlus.Services.Importers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp";
            });

            services.AddDbContext<Data.BankPlusContext>(options => options.UseSqlServer(Configuration.GetConnectionString("BankPlus")));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            AddAuthentication(services);

            services.AddAuthorization();
            services.AddAntiforgery((options) =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
            });

            RegisterServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IAntiforgery antiforgery)//, ILogger logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            /*using (StreamReader iisUrlRewriteConfig = File.OpenText("rewrite.config"))
            {
                var options = new RewriteOptions()
                    .AddIISUrlRewrite(iisUrlRewriteConfig);

                app.UseRewriter(options);
            }*/


            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.ContentType = "application/json";

                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

                    //logger.Log(LogLevel.Error, exceptionHandlerFeature.Error, exceptionHandlerFeature.Error.Message);

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(CreateProblemDetails(env, context, exceptionHandlerFeature.Error)));
                });
            });

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.Use(next => context =>
            {
                string path = context.Request.Path.Value;

                if (
                    String.Equals(path, "/", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(path, "/index.html", StringComparison.OrdinalIgnoreCase))
                {
                    // The request token can be sent as a JavaScript-readable cookie,
                    // and Angular uses it by default.
                    var tokens = antiforgery.GetAndStoreTokens(context);
                    context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken,
                        new CookieOptions() { HttpOnly = false });
                }

                return next(context);
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute("All", "{*url}", new { controller = "Home", action = "Index" });
                /*routes.MapRoute(
                    name: "default",
                    template: "{controller=home}/{action=Index}/{id?}");*/
            });

            app.UseSpa(spa =>
            {
                spa.Options.DefaultPage = "/";
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    //spa.UseReactDevelopmentServer(npmScript: "start");
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
                }
            });
        }

        private void AddAuthentication(IServiceCollection services)
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

            /*.AddAzureADBearer(null, JwtBearerDefaults.AuthenticationScheme, options =>
        {
            //
            options.OpenIdConnectSchemeName = JwtBearerDefaults.AuthenticationScheme;
            options.Domain = "mclachlan.family";
            options.TenantId = "30efefb9-9034-4e0c-8c69-17f4578f5924";
            //options.Authority = "https://login.microsoftonline.com/30efefb9-9034-4e0c-8c69-17f4578f5924/v2.0";
            //options.Audience = "045f8afa-70f2-4700-ab75-77ac41b306f7";
            options.ClientId = "045f8afa-70f2-4700-ab75-77ac41b306f7";
            options.ClientSecret = "gvwLXheN2Ba2OKFE*AxKi9jupyNq6.]+";
            //options.TokenValidationParameters.ValidateLifetime = true;
            //options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
        });*/
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ITransactionTagRepository, TransactionTagRepository>();
            services.AddScoped<ITransactionTagRuleRepository, TransactionTagRuleRepository>();
            services.AddScoped<IngImporter>();
            services.AddScoped<IImporterFactory, ImporterFactory>();
        }

        private ProblemDetails CreateProblemDetails(IHostingEnvironment env, HttpContext context, Exception ex)
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

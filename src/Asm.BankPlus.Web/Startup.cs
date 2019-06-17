using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            /*services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "wwwroot";
            });*/

            services.AddDbContext<Data.BankPlusContext>(options => options.UseSqlServer(Configuration.GetConnectionString("BankPlus")));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            //services.AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme).AddAzureADBearer((options) => Configuration.Bind("AzureAD", options));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = "https://login.microsoftonline.com/30efefb9-9034-4e0c-8c69-17f4578f5924/v2.0";
                options.Audience = "045f8afa-70f2-4700-ab75-77ac41b306f7";
                options.TokenValidationParameters.ValidateLifetime = true;
                options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
            });

            services.AddAuthorization();
            //api://045f8afa-70f2-4700-ab75-77ac41b306f7/

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseStaticFiles();
            //app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute("All", "{*url}", new { controller = "Home", action = "Index" });
                /*routes.MapRoute(
                    name: "default",
                    template: "{controller=home}/{action=Index}/{id?}");*/
            });

           /* app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });*/
        }
    }
}

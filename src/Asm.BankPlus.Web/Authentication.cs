using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asm.BankPlus.Web.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Asm.BankPlus.Web
{
    public static class Authentication
    {
        public static AuthenticationBuilder AddAzureAdBearer(this AuthenticationBuilder builder) => builder.AddAzureAdBearer(_ => { });

        public static AuthenticationBuilder AddAzureAdBearer(this AuthenticationBuilder builder, Action<AzureAppConfiguration> configureOptions)
        {

            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureAzureOptions>();
            builder.AddJwtBearer(options => {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        string accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;
                        if (!String.IsNullOrEmpty(accessToken))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },

                    /*OnTokenValidated = async context =>
                    {
                    }*/

                };
            });

            return builder;
        }

        private class ConfigureAzureOptions : IConfigureNamedOptions<JwtBearerOptions>
        {
            private readonly AzureAppConfiguration _azureOptions;
            private readonly IHostingEnvironment environment;

            public ConfigureAzureOptions(IOptions<AzureAppConfiguration> azureOptions, IHostingEnvironment environment)
            {
                _azureOptions = azureOptions.Value;
                this.environment = environment;
            }

            public void Configure(JwtBearerOptions options)
            {
                Configure(Options.DefaultName, options);
            }
            public void Configure(string name, JwtBearerOptions options)
            {
                options.Audience = _azureOptions.Audience;
                options.Authority = $"{_azureOptions.ADAuthority}{_azureOptions.TenantId}/v2.0";

                if (environment.IsDevelopment())
                {
                    options.RequireHttpsMetadata = false;
                }

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                };
            }

        }
    }
}

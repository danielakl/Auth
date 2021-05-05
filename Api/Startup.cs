using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Api
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
            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "Api", Version = "v1"}); });

            services.AddDbContext<DbContext>(opts =>
            {
                opts.UseInMemoryDatabase(nameof(DbContext));
                opts.UseOpenIddict();
            });

            services.AddOpenIddict()
                // Register OpenIddict core components
                .AddCore(opts =>
                {
                    opts.UseEntityFrameworkCore().UseDbContext<DbContext>();
                })
                // Register OpenIddict server components
                .AddServer(opts =>
                {
                    opts.AllowClientCredentialsFlow();
                    opts.SetTokenEndpointUris("/connect/token");
                    
                    // Encryption and signing of tokens
                    // TODO: Ephemeral Keys are discarded on application shutdown
                    opts.AddEphemeralEncryptionKey()
                        .AddEphemeralSigningKey();
                    // Token is encrypted by default
                    //    .DisableAccessTokenEncryption();

                    // Register scopes (permissions)
                    opts.RegisterScopes("api");

                    // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                    opts.UseAspNetCore().EnableTokenEndpointPassthrough();
                });

            services.AddHostedService<TestData>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
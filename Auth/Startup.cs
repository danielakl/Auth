using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Auth
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
            services.AddControllersWithViews();

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
                    opts.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();

                    opts.SetAuthorizationEndpointUris("/api/connect/authorize")
                        .SetTokenEndpointUris("/api/connect/token");

                    // Encryption and signing of tokens
                    // TODO: Ephemeral Keys are discarded on application shutdown
                    opts.AddEphemeralEncryptionKey()
                        .AddEphemeralSigningKey();
                    // Token is encrypted by default
                    //    .DisableAccessTokenEncryption();

                    // Register scopes (permissions)
                    opts.RegisterScopes("api");

                    // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                    opts.UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableTokenEndpointPassthrough();
                });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opts =>
                {
                    opts.LoginPath = "/account/login";
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
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
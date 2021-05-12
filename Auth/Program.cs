namespace Auth
{
    using System;
    using System.Threading.Tasks;
    using Auth.Database;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                // Build Host
                var hostBuilder = CreateHostBuilder(args).Build();

                // Create migration scope
                using (var scope = hostBuilder.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AuthContext>();
                    var options = scope.ServiceProvider.GetRequiredService<AuthDbOptions>();

                    // Migrate DB
                    // ----------------------------------------------------------------------------------------
                    // Reasons for NOT doing migration during startup
                    // ----------------------------------------------------------------------------------------
                    // - Multiple threads/processes/servers may attempt to migrate the database concurrently
                    // - Applications may try to access inconsistent state while this is happening
                    // - Usually the database permissions to modify the schema should not be granted for application execution
                    // - Its hard to revert back to a clean state if something goes wrong
                    // ----------------------------------------------------------------------------------------
                    if (options.MigrateDb)
                    {
                        await Migration.Migrate(context);
                    }
                }

                await hostBuilder.RunAsync();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}

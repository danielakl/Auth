namespace Auth.Database
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Database migration operations.
    /// </summary>
    public static class Migration
    {
        /// <summary>
        ///   Migrate database.
        /// </summary>
        /// <remarks>
        ///   There are good reasons for not doing migrations with code:
        ///   <list type="bullet">
        ///     <item><description>Multiple threads/processes/servers may attempt to migrate the database concurrently.</description></item>
        ///     <item><description>Applications may try to access inconsistent state while this is happening.</description></item>
        ///     <item><description>Usually the database permissions to modify the schema should not be granted for application execution.</description></item>
        ///     <item><description>Its hard to revert back to a clean state if something goes wrong.</description></item>
        ///   </list>
        /// </remarks>
        /// <param name="context">Database context to run migrations on.</param>
        /// <returns>A value indicating whether or not the migration went well.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="context"/> is <c>null</c>.</exception>
        public static async Task<bool> Migrate(AuthContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var totalMigrations = context.Database.GetMigrations().Count();
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    // Try to Migrate
                    await context.Database.MigrateAsync();

                    // Checking Applied Migrations
                    var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                    var appliedMigrationsCount = appliedMigrations.Count();

                    if (appliedMigrationsCount != totalMigrations)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(6));
                    }
                    else
                    {
                        // Migration Succeeded
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(6));
                }
            }

            // Migration Failed
            return false;
        }
    }
}

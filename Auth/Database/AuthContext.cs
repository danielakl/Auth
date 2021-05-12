namespace Auth.Database
{
    using Auth.Database.Entities;
    using Microsoft.EntityFrameworkCore;

    public class AuthContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthContext"/> class.
        /// </summary>
        /// <param name="options">Database context options.</param>
        public AuthContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var authContextAssembly = typeof(AuthContext).Assembly;
            modelBuilder.ApplyConfigurationsFromAssembly(authContextAssembly);
        }
    }
}

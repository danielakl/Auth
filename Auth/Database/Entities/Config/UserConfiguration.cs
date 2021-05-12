namespace Auth.Database.Entities.Config
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.AddedDate).HasDefaultValueSql(SqlConstants.SqlUtcNow);
            builder.Property(u => u.ModifiedDate).HasDefaultValueSql(SqlConstants.SqlUtcNow);
            builder.Property(u => u.Username).IsRequired().HasMaxLength(32);
            builder.ToTable("users");
        }
    }
}

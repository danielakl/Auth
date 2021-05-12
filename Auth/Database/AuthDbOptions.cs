namespace Auth.Database
{
    public record AuthDbOptions
    {
        public const string ConfigKey = "AuthDbOptions";

        public string ConnectionString { get; set; }

        public bool EnableDetailedErrors { get; set; }

        public bool EnableSensitiveDataLogging { get; set; }

        public bool MigrateDb { get; set; }

        public bool SeedDb { get; set; }
    }
}

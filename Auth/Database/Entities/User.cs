namespace Auth.Database.Entities
{
    using System;

    public record User
    {
        public Guid Id { get; set; }

        public DateTime AddedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string Username { get; set; }
    }
}

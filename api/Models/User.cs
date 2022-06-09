namespace api.Models
{
    using MongoDB.Bson.Serialization.Attributes;

    public class User
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ProfilePicture { get; set; }

        public string CoverPicture { get; set; }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public List<string> Followers { get; set; }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public List<string> Following { get; set; }

        public string Description { get; set; }

        public string City { get; set; }

        public string From { get; set; }

        public string Relationship { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

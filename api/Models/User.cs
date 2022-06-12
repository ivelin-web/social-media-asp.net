namespace api.Models
{
    using MongoDB.Bson.Serialization.Attributes;
    using System.ComponentModel.DataAnnotations;

    public class User
    {
        private const string defaultRelationship = "Single";

        public User(string email, string username, string password)
        {
            this.Email = email;
            this.Username = username;   
            this.Password = password;
        }

        [BsonId]
        [BsonElement("_id")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("username")]
        [Required]
        [MinLength(3)]
        public string Username { get; set; }

        [BsonElement("email")]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [BsonElement("password")]
        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [BsonElement("profilePicture")]
        public string ProfilePicture { get; set; } = "";

        [BsonElement("coverPicture")]
        public string CoverPicture { get; set; } = "";

        [BsonElement("followers")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public List<string> Followers { get; set; } = new List<string>();

        [BsonElement("following")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public List<string> Following { get; set; } = new List<string>();

        [BsonElement("description")]
        [MinLength(3)]
        [MaxLength(100)]
        public string Description { get; set; }

        [BsonElement("city")]
        [MinLength(3)]
        [MaxLength(30)]
        public string City { get; set; }

        [BsonElement("from")]
        [MinLength(3)]
        [MaxLength(30)]
        public string From { get; set; }

        [BsonElement("relationship")]
        public string Relationship { get; set; } = defaultRelationship;

        [BsonElement("createdAt")]
        public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyyMMddHHmmssffff");
    }
}

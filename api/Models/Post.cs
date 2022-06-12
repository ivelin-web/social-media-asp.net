using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
    public class Post
    {
        [BsonId]
        [BsonElement("_id")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string UserId { get; set; }

        public string Description { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public List<string> Likes { get; set; }

        public List<Object> Comments { get; set; }

        public string Img { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}

namespace api.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using System.ComponentModel.DataAnnotations;

    public class Post
    {
        [BsonId]
        [BsonElement("_id")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string _id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("userId")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonElement("description")]
        [MaxLength(500)]
        [MinLength(3)]
        public string Description { get; set; }

        [BsonElement("likes")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public List<string> Likes { get; set; } = new List<string>();

        [BsonElement("comments")]
        public List<Comment> Comments { get; set; } = new List<Comment>();

        [BsonElement("img")]
        public string Img { get; set; } 

        [BsonElement("createdAt")]
        public string CreatedAt { get; set; } = Convert.ToString((int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
    }
}

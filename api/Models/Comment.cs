namespace api.Models
{
    using MongoDB.Bson.Serialization.Attributes;
    using System.ComponentModel.DataAnnotations;
    using MongoDB.Bson;

    public class Comment
    {
        [BsonId]
        [BsonElement("_id")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string _id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("text")]
        [Required]
        [MaxLength(50)]
        [MinLength(3)]
        public string Text { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        [BsonElement("owner")]
        public string Owner { get; set; }

        [BsonElement("createdAt")]
        public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyyMMddHHmmssffff");
    }
}
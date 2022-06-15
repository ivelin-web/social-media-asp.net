using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class Comment
    {
        [BsonElement("text")]
        [Required]
        [MaxLength(50)]
        [MinLength(3)]
        public string Text { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        [BsonElement("owner")]
        [Required]
        public string Owner { get; set; }

        [BsonElement("createdAt")]
        public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyyMMddHHmmssffff");
    }
}

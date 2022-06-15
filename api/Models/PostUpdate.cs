namespace api.Models
{
    public class PostUpdate
    {
        public string Description { get; set; } = null;

        public List<string> Likes { get; set; } = null;

        public List<Comment> Comments { get; set; } = null;

        public string Img { get; set; } = null;
    }
}

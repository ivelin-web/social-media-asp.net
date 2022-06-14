namespace api.Models;

public class UserUpdate
{
    public string Username { get; set; } = null;

    public string Email { get; set; } = null;

    public string Password { get; set; } = null;

    public string ProfilePicture { get; set; } = null;

    public string CoverPicture { get; set; } = null;

    public List<string> Followers { get; set; } = null;

    public List<string> Following { get; set; } = null;

    public string Description { get; set; } = null;

    public string City { get; set; } = null;

    public string From { get; set; } = null;

    public string Relationship { get; set; } = null;

    public string CreatedAt { get; set; } = null;
}
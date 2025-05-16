namespace IndieSphere.Domain.User;

public class OAuthUser
{
    public string Id { get; set; } // Your internal user ID
    public string GoogleId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string ProfilePicture { get; set; }
    public bool EmailVerified { get; set; }
    public string Locale { get; set; }
    public DateTime CreatedAt { get; set; }
}

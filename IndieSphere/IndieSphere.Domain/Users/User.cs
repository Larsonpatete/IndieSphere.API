using System.ComponentModel.DataAnnotations;

namespace IndieSphere.Domain.Users;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    public string SpotifyId { get; set; }

    [Required]
    [MaxLength(255)]
    public string DisplayName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    // Store tokens securely on the server, not in the JWT
    public string SpotifyAccessToken { get; set; }
    public string SpotifyRefreshToken { get; set; }
    public DateTime? AccessTokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;
}

namespace IndieSphere.Domain.User;

public class User
{
    public int Id { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;

    //public User(string username, string email)
    //{
    //    if (string.IsNullOrWhiteSpace(username))
    //        throw new ArgumentException("Username cannot be empty", nameof(username));

    //    if (string.IsNullOrWhiteSpace(email))
    //        throw new ArgumentException("Email cannot be empty", nameof(email));

    //    Username = username;
    //    Email = email;
    //}

    //// Protected constructor for ORM
    protected User() { }

    //public void UpdateEmail(string newEmail)
    //{
    //    if (string.IsNullOrWhiteSpace(newEmail))
    //        throw new ArgumentException("Email cannot be empty", nameof(newEmail));

    //    Email = newEmail;
    //}
}

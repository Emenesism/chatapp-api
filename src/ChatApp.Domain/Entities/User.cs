namespace ChatApp.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string PhoneNumber { get; private set; }
    public string? PasswordHash { get; private set; }
    public DateTime CreateAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public User(string name, string phoneNumber, string passwordHash)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;

    }
}

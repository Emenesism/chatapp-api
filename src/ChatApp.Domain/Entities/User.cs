namespace ChatApp.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string PhoneNumber { get; private set; }
    public string? PasswordHash { get; private set; }
    public DateTime CreateAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<Message> Messages { get; private set; } = new List<Message>();


    public User(string name, string phoneNumber, string passwordHash)
    {
        Id = Guid.NewGuid();
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Name is required", nameof(name)) : name.Trim();
        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? throw new ArgumentException("Phone is required", nameof(phoneNumber)) : phoneNumber.Trim();
        PasswordHash = string.IsNullOrWhiteSpace(passwordHash) ? throw new ArgumentException("Password hash is required", nameof(passwordHash)) : passwordHash;

        CreateAt = DateTime.UtcNow;
        UpdatedAt = CreateAt;
    }
}

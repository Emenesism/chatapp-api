namespace TicketSystem.Api.Contracts.Responses;

public class CreateAdminReponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = [];
    public bool IsSuperAdmin { get; set; }
}

public class UpdateAdminResponse
{
    public string Name { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

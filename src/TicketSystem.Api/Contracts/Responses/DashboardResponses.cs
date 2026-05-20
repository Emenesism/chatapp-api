namespace TicketSystem.Api.Contracts.Responses;

public sealed class AllSessionResponse
{
    public Guid Id { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastTimeUsed { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid? AdminId { get; set; }
    public Guid? UserId { get; set; }
    public bool IsAdmin { get; set; }
}

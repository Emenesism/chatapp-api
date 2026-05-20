namespace TicketSystem.Api.Contracts.Responses;

public class MessageResponse
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid TicketId { get; set; }
    public Guid SenderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<FileResponse> Attachments { get; set; } = [];
}

public class FileResponse
{
    public Guid Id { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
}

namespace ChatApp.Application.Messages.PV;

public record SendPvMessageResult(
    Guid Id,
    Guid SenderId,
    Guid ReceiverId,
    string Content,
    DateTime CreatedAt
);

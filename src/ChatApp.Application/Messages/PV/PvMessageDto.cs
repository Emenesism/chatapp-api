namespace ChatApp.Application.Messages.PV;

public record PvMessageDto(
    Guid Id,
    Guid SenderId,
    Guid ReceiverId,
    string Content,
    DateTime CreatedAt
);

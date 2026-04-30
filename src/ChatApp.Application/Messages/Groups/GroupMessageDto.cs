namespace ChatApp.Application.Messages.Groups;

public record GroupMessageDto(
    Guid Id,
    Guid GroupId,
    string GroupUsername,
    Guid SenderId,
    string Content,
    DateTime CreatedAt
);

namespace ChatApp.Application.Messages.Groups;

public record SendGroupMessageResult(
    Guid Id,
    Guid GroupId,
    string GroupUsername,
    Guid SenderId,
    string Content,
    DateTime CreatedAt
);

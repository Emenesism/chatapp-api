namespace ChatApp.Application.Messages.Groups;

public record CreateGroupChatResult(
    Guid Id,
    string Name,
    string Username,
    Guid CreatorId,
    DateTime CreatedAt
);

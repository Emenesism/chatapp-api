namespace ChatApp.Application.Messages.Groups;

public record CreateGroupChatRequest(
    string Name,
    string Username
);

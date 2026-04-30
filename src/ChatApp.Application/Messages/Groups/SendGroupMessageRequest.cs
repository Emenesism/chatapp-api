namespace ChatApp.Application.Messages.Groups;

public record SendGroupMessageRequest(
    string GroupUsername,
    string Content
);

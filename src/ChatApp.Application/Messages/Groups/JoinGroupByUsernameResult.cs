namespace ChatApp.Application.Messages.Groups;

public record JoinGroupByUsernameResult(
    Guid GroupId,
    string GroupName,
    string GroupUsername,
    bool AlreadyMember
);

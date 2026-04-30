namespace ChatApp.Application.Messages.Groups;

public record GetGroupHistoryQuery(
    string GroupUsername,
    DateTime? Before = null,
    int Take = 50
);

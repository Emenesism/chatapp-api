namespace ChatApp.Application.Messages.PV;

public record GetPvHistoryQuery(Guid OtherUserId, DateTime? Before = null, int Take = 50);

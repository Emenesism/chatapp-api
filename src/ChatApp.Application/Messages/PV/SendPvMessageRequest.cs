namespace ChatApp.Application.Messages.PV;

public record SendPvMessageRequest(Guid ReceiverId, string Content);

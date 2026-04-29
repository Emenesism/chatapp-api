using ChatApp.Application.Abstractions.Repositories;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Messages.PV;

public class SendPvMessageHandler
{
    private readonly IMessageRepository _messages;
    private readonly IUserRepository _users;

    public SendPvMessageHandler(IMessageRepository messages, IUserRepository users)
    {
        _messages = messages;
        _users = users;
    }

    public async Task<SendPvMessageResult> Handle(Guid senderId, SendPvMessageRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (senderId == Guid.Empty) throw new ArgumentException("Sender id is required", nameof(senderId));
        if (request.ReceiverId == Guid.Empty) throw new ArgumentException("Receiver id is required", nameof(request.ReceiverId));
        if (senderId == request.ReceiverId) throw new InvalidOperationException("You cannot send message to yourself");

        var receiver = await _users.GetByIdAsync(request.ReceiverId);
        if (receiver is null)
            throw new InvalidOperationException("Receiver not found");

        var message = new Message(senderId, request.ReceiverId, request.Content);
        await _messages.AddAsync(message);

        return new SendPvMessageResult(
            message.Id,
            message.SenderId,
            message.ReceiverId,
            message.Content,
            message.CreatedAt
        );
    }
}

using ChatApp.Application.Abstractions.Repositories;

namespace ChatApp.Application.Messages.PV;

public class GetPvHistoryHandler
{
    private readonly IMessageRepository _messages;
    private readonly IUserRepository _users;

    public GetPvHistoryHandler(IMessageRepository messages, IUserRepository users)
    {
        _messages = messages;
        _users = users;
    }

    public async Task<IReadOnlyList<PvMessageDto>> Handle(Guid currentUserId, GetPvHistoryQuery query)
    {
        if (query is null) throw new ArgumentNullException(nameof(query));
        if (currentUserId == Guid.Empty) throw new ArgumentException("Current user id is required", nameof(currentUserId));
        if (query.OtherUserId == Guid.Empty) throw new ArgumentException("Other user id is required", nameof(query.OtherUserId));
        if (currentUserId == query.OtherUserId) throw new InvalidOperationException("Self history is not supported");

        var otherUser = await _users.GetByIdAsync(query.OtherUserId);
        if (otherUser is null)
            throw new InvalidOperationException("User not found");

        var take = Math.Clamp(query.Take, 1, 100);
        var messages = await _messages.GetPvHistoryAsync(currentUserId, query.OtherUserId, query.Before, take);

        return messages
            .Select(m => new PvMessageDto(m.Id, m.SenderId, m.ReceiverId, m.Content, m.CreatedAt))
            .ToList();
    }
}

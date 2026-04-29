using ChatApp.Application.Abstractions.Repositories;

namespace ChatApp.Application.Users.GetUsers;

public class GetUsersHandler
{
    private readonly IUserRepository _users;

    public GetUsersHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<IReadOnlyList<UserSummaryDto>> Handle()
    {
        var users = await _users.GetAllAsync();
        return users
            .Select(u => new UserSummaryDto(u.Id, u.Name, u.PhoneNumber))
            .ToList();
    }
}

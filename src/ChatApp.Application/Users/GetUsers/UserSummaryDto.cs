namespace ChatApp.Application.Users.GetUsers;

public record UserSummaryDto(
    Guid Id,
    string Name,
    string PhoneNumber
);

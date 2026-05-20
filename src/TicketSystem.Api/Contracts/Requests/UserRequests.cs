using System.ComponentModel.DataAnnotations;

namespace TicketSystem.Api.Contracts.Requests;

public class CreateUserDto
{
    [Required(ErrorMessage = "Name Property Is Required")]
    [MinLength(3, ErrorMessage = "Name Must At Least 3 Char")]
    [MaxLength(50, ErrorMessage = "Name Is Too Long")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username Property Is Required")]
    [MinLength(3, ErrorMessage = "Username Must At Least 3 Char")]
    [MaxLength(50, ErrorMessage = "Username Is Too Long")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password Property Is Required")]
    [MinLength(8, ErrorMessage = "Password Must At Least 8 Char")]
    public string Password { get; set; } = string.Empty;
}

public class PaginationDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page Must Be At Least 1")]
    public int Page { get; set; } = 1; //Default Value For Page

    [Range(1, int.MaxValue, ErrorMessage = "Limit Must Be At Least 1")]
    public int Limit { get; set; } = 10; //Default Value For Limit
}

public class GetUsersFilterDto : PaginationDto
{
    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }
}

public class UpdateDto
{
    [MinLength(3, ErrorMessage = "Username Must At Least 3 Char")]
    [MaxLength(50, ErrorMessage = "Username Is Too Long")]
    public string Username { get; set; } = string.Empty;

    [MinLength(8, ErrorMessage = "Password Must At Least 8 Char")]
    public string Password { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace TicketSystem.Api.Contracts.Requests;

public class CreateAdminDto
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

    [Required(ErrorMessage = "IsSuperAdmin Is Required")]
    public bool IsSuperAdmin { get; set; }

    [Required(ErrorMessage = "Roles Are Requierd.")]
    [MinLength(1, ErrorMessage = "At Least Choose One Role")]
    public List<string> Roles { get; set; } = [];
}

public class LoginAdminDto
{
    [Required(ErrorMessage = "Username Property Is Required")]
    [MinLength(3, ErrorMessage = "Username Must At Least 3 Char")]
    [MaxLength(50, ErrorMessage = "Username Is Too Long")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password Property Is Required")]
    [MinLength(8, ErrorMessage = "Password Must At Least 8 Char")]
    public string Password { get; set; } = string.Empty;
}

public class UpdateAdminDto
{
    [MinLength(3, ErrorMessage = "Name Must At Least 3 Char")]
    [MaxLength(50, ErrorMessage = "Name Is Too Long")]
    public string Name { get; set; } = string.Empty;

    [MinLength(8, ErrorMessage = "Password Must At Least 8 Char")]
    public string Password { get; set; } = string.Empty;
}

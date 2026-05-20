using System.ComponentModel.DataAnnotations;

namespace TicketSystem.Api.Contracts.Requests;

public class CreateTicketDto
{
    [Required(ErrorMessage = "Title For Ticket Is Required")]
    [MinLength(1, ErrorMessage = "Title Is Too Short")]
    [MaxLength(60, ErrorMessage = "Title Is Too Long")]
    public string Title { get; set; } = string.Empty;
}

public class TicketIdDto
{
    [Required(ErrorMessage = "TicketId Is Required")]
    public Guid TicketId { get; set; }
}

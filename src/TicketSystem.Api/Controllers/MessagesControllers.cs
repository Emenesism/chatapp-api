using TicketSystem.Domain.Entities;
using TicketSystem.Application.Abstractions.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TicketSystem.Application.Dtos.Messages;
using System.Security.Claims;
using TicketSystem.Application.Dtos.Tickets;
using TicketSystem.Application.Common.Exceptions;
using TicketSystem.Application.Common.Interface;
using TicketSystem.Application.Dtos.Notification;
using TicketSystem.Application.Dtos.File;
using TicketSystem.Application.Services;


[ApiController]
[Route("message")]
[Authorize]
public class MessageController(
    ITicketMessageRepository messageRepository,
    ITicketRepository ticketRepository,
    INotificationRepository notificationRepository,
    INotificationSender notificationSender,
    IFileStorageService fileStorageService) : ControllerBase
{
    private const int MaxFileBytes = 10 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".pdf", ".docx"];

    [HttpPost]
    public async Task<ActionResult<MessageResponse>> CreateMessage([FromBody] CreateMessageDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var senderId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var ticket = await ticketRepository.GetTicketViaTicketId(dto.TicketId);

        if (ticket is null)
        {
            throw new NotFoundException("Ticket not found");
        }

        var message = new TicketMessage(dto.Content, senderId, dto.TicketId);

        await messageRepository.CreateTicketMessage(message);

        var receiverId = senderId == ticket.UserId ? ticket.AdminId : ticket.UserId;
        if (receiverId is not null && receiverId != senderId)
        {
            var notification = new Notification(
                receiverId.Value.ToString(),
                "New ticket message",
                $"New message on ticket: {ticket.Title}");

            await notificationRepository.AddAsync(notification);

            await notificationSender.SendToUserAsync(receiverId.Value, new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Body = notification.Body,
                CreatedAt = notification.CreatedAt
            });
        }

        return Ok(new MessageResponse
        {
            Id = message.Id,
            SenderId = message.SenderId,
            TicketId = message.TicketId,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            Attachments = []
        });
    }

    [HttpPost("{messageId:guid}/attachment")]
    [RequestSizeLimit(MaxFileBytes)]
    public async Task<ActionResult<FileResponse>> UploadAttachment(Guid messageId, [FromForm] IFormFile file)
    {
        if (file is null)
        {
            return BadRequest("No file provided.");
        }

        var senderId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var message = await messageRepository.GetMessageById(messageId);

        if (message is null)
        {
            throw new NotFoundException("Message not found");
        }

        if (message.SenderId != senderId)
        {
            throw new ForbiddenException("You can only attach files to your own messages");
        }

        var dto = new UploadFileDto
        {
            FileName = file.FileName,
            ContentType = file.ContentType
        };

        var validator = new FileValidator(MaxFileBytes, AllowedExtensions);
        validator.Validate(dto, file.Length);

        var extension = Path.GetExtension(dto.FileName).ToLowerInvariant();
        using var stream = file.OpenReadStream();
        var storageName = await fileStorageService.SaveAsync(stream, dto.ContentType, extension);

        var attachment = new Attachment(dto.FileName, file.Length, dto.ContentType)
        {
            TicketMessageId = messageId,
            StorageName = storageName
        };

        try
        {
            await messageRepository.CreateAttachment(attachment);
        }
        catch
        {
            await fileStorageService.DeleteAsync(storageName);
            throw;
        }

        return Ok(new FileResponse
        {
            Id = attachment.Id,
            Filename = attachment.Filename,
            ContentType = attachment.ContentType,
            Size = attachment.Size,
            CreatedAt = attachment.CreatedAt
        });
    }

    [HttpGet("attachment/{attachmentId:guid}")]
    public async Task<IActionResult> DownloadAttachment(Guid attachmentId)
    {
        var attachment = await messageRepository.GetAttachmentById(attachmentId);

        if (attachment is null)
        {
            throw new NotFoundException("Attachment not found");
        }

        Stream stream;
        try
        {
            stream = await fileStorageService.GetStreamAsync(attachment.StorageName);
        }
        catch (FileNotFoundException)
        {
            throw new NotFoundException("Attachment file not found");
        }

        return File(stream, attachment.ContentType, attachment.Filename);
    }

    [HttpDelete("attachment/{attachmentId:guid}")]
    public async Task<ActionResult> DeleteAttachment(Guid attachmentId)
    {
        var senderId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var attachment = await messageRepository.GetAttachmentById(attachmentId);

        if (attachment is null)
        {
            throw new NotFoundException("Attachment not found");
        }

        if (attachment.TicketMessage.SenderId != senderId)
        {
            throw new ForbiddenException("You can only delete your own attachments");
        }

        await fileStorageService.DeleteAsync(attachment.StorageName);
        await messageRepository.DeleteAttachment(attachment);

        return Ok(new
        {
            message = "Done"
        });
    }

    [HttpPost("all")]
    public async Task<ActionResult<List<MessageResponse>>> GetAllMessageRelatedToTicket([FromBody] TicketIdDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem();
        }

        var tickets = await messageRepository.GetAllMessageRelatedOfTicket(dto.TicketId);

        var result = tickets.Select(s => new MessageResponse
        {
            Content = s.Content,
            Id = s.Id,
            SenderId = s.SenderId,
            CreatedAt = s.CreatedAt,
            TicketId = s.TicketId,
            Attachments = s.Attachments.Select(a => new FileResponse
            {
                Id = a.Id,
                Filename = a.Filename,
                ContentType = a.ContentType,
                Size = a.Size,
                CreatedAt = a.CreatedAt
            }).ToList()
        }).ToList();

        return Ok(result);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteMessage([FromBody] MessageIdDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem();
        }

        var senderId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var status = await messageRepository.DeleteMessage(dto.MessageId, senderId);

        if (!status)
        {
            throw new NotFoundException("The Message Is Not Found Or You Delete Another Person Message");
            // return NotFound(new
            // {
            //     message = "The Message Is Not Found Or You Delete Another Person Message"
            // });
        }

        return Ok(new
        {
            message = "Done"
        });

    }
}

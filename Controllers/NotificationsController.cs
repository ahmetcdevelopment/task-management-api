using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TaskManagement.API.Services;
using TaskManagement.API.DTOs;
using TaskManagement.API.Hubs;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationsController(
        INotificationService notificationService,
        IHubContext<NotificationHub> hubContext)
    {
        _notificationService = notificationService;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetNotifications([FromQuery] NotificationFilterDto filter)
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);

            var notificationDtos = notifications.Select(MapToResponseDto).ToList();
            return Ok(notificationDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Bildirimler alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("unread")]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetUnreadNotifications()
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);

            var notificationDtos = notifications.Select(MapToResponseDto).ToList();
            return Ok(notificationDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Okunmamış bildirimler alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<long>> GetUnreadCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Okunmamış bildirim sayısı alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<NotificationResponseDto>> CreateNotification(CreateNotificationDto createNotificationDto)
    {
        try
        {
            var notification = await _notificationService.CreateNotificationAsync(createNotificationDto);

            // Real-time bildirim gönder
            var realTimeNotification = new RealTimeNotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = (DTOs.NotificationType)notification.Type,
                CreatedAt = notification.CreatedAt,
                ActionUrl = notification.ActionUrl,
                Metadata = notification.Metadata
            };

            await _hubContext.SendNotificationToUserAsync(notification.UserId, realTimeNotification);

            return CreatedAtAction(nameof(GetNotifications), MapToResponseDto(notification));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Bildirim oluşturulurken hata oluştu", error = ex.Message });
        }
    }

    [HttpPatch("{id}/mark-as-read")]
    public async Task<ActionResult> MarkAsRead(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _notificationService.MarkAsReadAsync(id);

            if (!success)
                return NotFound(new { message = "Bildirim bulunamadı" });

            // Güncellenmiş okunmamış sayıyı gönder
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            await _hubContext.UpdateUnreadCountAsync(userId, unreadCount);

            return Ok(new { message = "Bildirim okundu olarak işaretlendi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Bildirim güncellenirken hata oluştu", error = ex.Message });
        }
    }

    [HttpPatch("mark-all-as-read")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _notificationService.MarkAllAsReadAsync(userId);

            if (!success)
                return BadRequest(new { message = "Bildirimler güncellenemedi" });

            // Okunmamış sayıyı sıfırla
            await _hubContext.UpdateUnreadCountAsync(userId, 0);

            return Ok(new { message = "Tüm bildirimler okundu olarak işaretlendi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Bildirimler güncellenirken hata oluştu", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteNotification(string id)
    {
        try
        {
            var success = await _notificationService.DeleteNotificationAsync(id);

            if (!success)
                return NotFound(new { message = "Bildirim bulunamadı" });

            return Ok(new { message = "Bildirim silindi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Bildirim silinirken hata oluştu", error = ex.Message });
        }
    }

    [HttpPost("bulk-action")]
    public async Task<ActionResult> BulkAction(BulkNotificationActionDto bulkActionDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var successCount = 0;

            foreach (var notificationId in bulkActionDto.NotificationIds)
            {
                bool success = false;

                switch (bulkActionDto.Action.ToLower())
                {
                    case "markasread":
                        success = await _notificationService.MarkAsReadAsync(notificationId);
                        break;
                    case "delete":
                        success = await _notificationService.DeleteNotificationAsync(notificationId);
                        break;
                }

                if (success) successCount++;
            }

            // Güncellenmiş okunmamış sayıyı gönder
            if (bulkActionDto.Action.ToLower() == "markasread")
            {
                var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
                await _hubContext.UpdateUnreadCountAsync(userId, unreadCount);
            }

            return Ok(new { message = $"{successCount} bildirim işlendi", processedCount = successCount });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Toplu işlem yapılırken hata oluştu", error = ex.Message });
        }
    }

    [HttpDelete("cleanup")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CleanupOldNotifications([FromQuery] int daysOld = 30)
    {
        try
        {
            var success = await _notificationService.DeleteOldNotificationsAsync(daysOld);

            if (!success)
                return BadRequest(new { message = "Eski bildirimler temizlenemedi" });

            return Ok(new { message = $"{daysOld} günden eski bildirimler temizlendi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Eski bildirimler temizlenirken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<NotificationSummaryDto>> GetNotificationSummary()
    {
        try
        {
            var userId = GetCurrentUserId();
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            var allNotifications = await _notificationService.GetUserNotificationsAsync(userId);

            var summary = new NotificationSummaryDto
            {
                TotalNotifications = allNotifications.Count(),
                UnreadNotifications = unreadCount,
                ReadNotifications = allNotifications.Count() - unreadCount,
                NotificationsByType = allNotifications
                    .GroupBy(n => n.Type)
                    .ToDictionary(g => (DTOs.NotificationType)g.Key, g => (long)g.Count())
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Bildirim özeti alınırken hata oluştu", error = ex.Message });
        }
    }

    // Test endpoint for sending notifications
    [HttpPost("test-send")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> SendTestNotification([FromBody] CreateNotificationDto testNotification)
    {
        try
        {
            var notification = await _notificationService.CreateNotificationAsync(testNotification);

            var realTimeNotification = new RealTimeNotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = (DTOs.NotificationType)notification.Type,
                CreatedAt = notification.CreatedAt,
                ActionUrl = notification.ActionUrl,
                Metadata = notification.Metadata
            };

            await _hubContext.SendNotificationToUserAsync(notification.UserId, realTimeNotification);

            return Ok(new { message = "Test bildirimi gönderildi", notificationId = notification.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Test bildirimi gönderilirken hata oluştu", error = ex.Message });
        }
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               User.FindFirst("userId")?.Value ??
               throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı");
    }

    private static NotificationResponseDto MapToResponseDto(Models.Notification notification)
    {
        return new NotificationResponseDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            Type = (DTOs.NotificationType)notification.Type,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt,
            RelatedEntityId = notification.RelatedEntityId,
            RelatedEntityType = notification.RelatedEntityType,
            ActionUrl = notification.ActionUrl,
            Metadata = notification.Metadata
        };
    }
}
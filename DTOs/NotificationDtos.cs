using System.ComponentModel.DataAnnotations;
using TaskManagement.API.Models;

namespace TaskManagement.API.DTOs;

public class CreateNotificationDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; } = NotificationType.Info;

    public string? RelatedEntityId { get; set; }

    public string? RelatedEntityType { get; set; }

    public string? ActionUrl { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }
}

public class NotificationResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? ActionUrl { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class MarkNotificationAsReadDto
{
    [Required]
    public string NotificationId { get; set; } = string.Empty;
}

public class NotificationFilterDto
{
    public string? UserId { get; set; }
    public NotificationType? Type { get; set; }
    public bool? IsRead { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? RelatedEntityType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

public class NotificationSummaryDto
{
    public long TotalNotifications { get; set; }
    public long UnreadNotifications { get; set; }
    public long ReadNotifications { get; set; }
    public Dictionary<NotificationType, long> NotificationsByType { get; set; } = new();
}

public class BulkNotificationActionDto
{
    [Required]
    public List<string> NotificationIds { get; set; } = new();
    
    [Required]
    public string Action { get; set; } = string.Empty; // "markAsRead", "delete"
}

public class NotificationPreferencesDto
{
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool TaskAssignedNotifications { get; set; } = true;
    public bool TaskUpdatedNotifications { get; set; } = true;
    public bool TaskCompletedNotifications { get; set; } = true;
    public bool ProjectNotifications { get; set; } = true;
    public bool ReminderNotifications { get; set; } = true;
}

public class RealTimeNotificationDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ActionUrl { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public enum NotificationType
{
    Info = 1,
    TaskAssigned = 2,
    TaskUpdated = 3,
    TaskCompleted = 4,
    ProjectCreated = 5,
    ProjectUpdated = 6,
    TeamMemberAdded = 7,
    TeamMemberRemoved = 8,
    DeadlineReminder = 9,
    SystemAlert = 10
}
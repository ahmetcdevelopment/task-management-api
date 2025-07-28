using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.Models;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; } = NotificationType.Info;

    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    public string? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? ActionUrl { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public enum NotificationType
{
    Info = 1,
    Warning = 2,
    Error = 3,
    Success = 4,
    TaskAssigned = 5,
    TaskUpdated = 6,
    TaskCompleted = 7,
    ProjectCreated = 8,
    ProjectUpdated = 9,
    Reminder = 10
}
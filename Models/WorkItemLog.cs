using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.Models;

public class WorkItemLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [BsonRepresentation(BsonType.ObjectId)]
    public string WorkItemId { get; set; } = string.Empty;

    [Required]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public WorkItemLogAction Action { get; set; }

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? FieldName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public enum WorkItemLogAction
{
    Created = 1,
    Updated = 2,
    StatusChanged = 3,
    AssigneeChanged = 4,
    CommentAdded = 5,
    AttachmentAdded = 6,
    AttachmentRemoved = 7,
    Deleted = 8,
    PriorityChanged = 9,
    DueDateChanged = 10
}

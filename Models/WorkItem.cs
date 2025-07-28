using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.Models;

public class WorkItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProjectId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string? AssignedToId { get; set; }

    [Required]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CreatedById { get; set; } = string.Empty;

    public WorkItemStatus Status { get; set; } = WorkItemStatus.ToDo;
    public WorkItemPriority Priority { get; set; } = WorkItemPriority.Medium;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int EstimatedHours { get; set; }
    public int ActualHours { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Attachments { get; set; } = new();
    public List<WorkItemComment> Comments { get; set; } = new();
}

public class WorkItemComment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum WorkItemStatus
{
    ToDo = 1,
    InProgress = 2,
    Done = 3,
    Cancelled = 5
}

public enum WorkItemPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

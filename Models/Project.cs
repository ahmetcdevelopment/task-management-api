using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.Models;

public class Project
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ManagerId { get; set; } = string.Empty;

    public List<string> TeamMemberIds { get; set; } = new();

    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public decimal Budget { get; set; }
    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;
    public List<string> Tags { get; set; } = new();
}

public enum ProjectStatus
{
    Planning = 1,
    InProgress = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5
}

public enum ProjectPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
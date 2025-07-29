using TaskManagement.API.Models;

namespace TaskManagement.API.DTOs;

public class WorkItemLogResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string WorkItemId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public WorkItemLogAction Action { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? FieldName { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

using System.ComponentModel.DataAnnotations;
using TaskManagement.API.Models;

namespace TaskManagement.API.DTOs;

// Create WorkItem DTO
public class CreateWorkItemDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string ProjectId { get; set; } = string.Empty;

    public string? AssignedToId { get; set; }

    public WorkItemPriority Priority { get; set; } = WorkItemPriority.Medium;

    public DateTime? DueDate { get; set; }

    public int EstimatedHours { get; set; }

    public List<string> Tags { get; set; } = new();
}

// Update WorkItem DTO
public class UpdateWorkItemDto
{
    [StringLength(200)]
    public string? Title { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public string? AssignedToId { get; set; }

    public WorkItemPriority? Priority { get; set; }

    public DateTime? DueDate { get; set; }

    public int? EstimatedHours { get; set; }

    public int? ActualHours { get; set; }

    public List<string>? Tags { get; set; }
}

// WorkItem Response DTO
public class WorkItemResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string? AssignedToId { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public WorkItemStatus Status { get; set; }
    public WorkItemPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int EstimatedHours { get; set; }
    public int ActualHours { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Attachments { get; set; } = new();
    public List<WorkItemCommentDto> Comments { get; set; } = new();
    
    // Navigation properties
    public string? AssignedToName { get; set; }
    public string? CreatedByName { get; set; }
    public string? ProjectName { get; set; }
}

// WorkItem Comment DTO
public class WorkItemCommentDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? UserName { get; set; }
}

// Add WorkItem Comment DTO
public class AddWorkItemCommentDto
{
    [Required]
    [StringLength(1000)]
    public string Comment { get; set; } = string.Empty;
}

// Assign WorkItem DTO
public class AssignWorkItemDto
{
    [Required]
    public string AssigneeId { get; set; } = string.Empty;
}

// Update WorkItem Status DTO
public class UpdateWorkItemStatusDto
{
    [Required]
    public WorkItemStatus Status { get; set; }
}

// WorkItem Filter DTO
public class WorkItemFilterDto
{
    public string? ProjectId { get; set; }
    public string? AssignedToId { get; set; }
    public WorkItemStatus? Status { get; set; }
    public WorkItemPriority? Priority { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public List<string>? Tags { get; set; }
    public string? Search { get; set; }
    public bool? IsOverdue { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// WorkItem Summary DTO
public class WorkItemSummaryDto
{
    public int TotalWorkItems { get; set; }
    public int ToDoWorkItems { get; set; }
    public int InProgressWorkItems { get; set; }
    public int CompletedWorkItems { get; set; }
    public int OverdueWorkItems { get; set; }
    public int WorkItemsDueSoon { get; set; }
    public Dictionary<WorkItemPriority, int> WorkItemsByPriority { get; set; } = new();
    public Dictionary<string, int> WorkItemsByProject { get; set; } = new();
}

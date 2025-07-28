using System.ComponentModel.DataAnnotations;
using TaskManagement.API.Models;

namespace TaskManagement.API.DTOs;

public class CreateProjectDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string ManagerId { get; set; } = string.Empty;

    public List<string>? TeamMemberIds { get; set; }

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime? EndDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Budget { get; set; }

    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;

    public List<string>? Tags { get; set; }
}

public class UpdateProjectDto
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? Budget { get; set; }

    public ProjectPriority? Priority { get; set; }

    public List<string>? Tags { get; set; }
}

public class ProjectResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ManagerId { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public List<ProjectTeamMemberDto> TeamMembers { get; set; } = new();
    public ProjectStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal Budget { get; set; }
    public ProjectPriority Priority { get; set; }
    public List<string> Tags { get; set; } = new();
    public ProjectStatsDto Stats { get; set; } = new();
}

public class ProjectTeamMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? ProfileImageUrl { get; set; }
}

public class ProjectStatsDto
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public double CompletionPercentage { get; set; }
    public int TotalEstimatedHours { get; set; }
    public int TotalActualHours { get; set; }
}

public class AddTeamMemberDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}

public class RemoveTeamMemberDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}

public class UpdateProjectStatusDto
{
    [Required]
    public ProjectStatus Status { get; set; }
}

public class ProjectFilterDto
{
    public string? ManagerId { get; set; }
    public string? TeamMemberId { get; set; }
    public ProjectStatus? Status { get; set; }
    public ProjectPriority? Priority { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public List<string>? Tags { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

public class ProjectSummaryDto
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int OnHoldProjects { get; set; }
    public int CancelledProjects { get; set; }
    public decimal TotalBudget { get; set; }
}
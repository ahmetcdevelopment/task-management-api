using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskManagement.API.Services;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;
using TaskManagement.API.Repositories;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkItemsController : ControllerBase
{
    private readonly IWorkItemService _workItemService;
    private readonly IProjectService _projectService;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public WorkItemsController(
        IWorkItemService workItemService,
        IProjectService projectService,
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _workItemService = workItemService;
        _projectService = projectService;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkItemResponseDto>>> GetWorkItems([FromQuery] WorkItemFilterDto filter)
    {
        try
        {
            var workItems = await _workItemService.GetAllWorkItemsAsync();
            var workItemDtos = new List<WorkItemResponseDto>();

            foreach (var workItem in workItems)
            {
                workItemDtos.Add(await MapToResponseDto(workItem));
            }

            return Ok(workItemDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Work items alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkItemResponseDto>> GetWorkItem(string id)
    {
        try
        {
            var workItem = await _workItemService.GetWorkItemByIdAsync(id);
            if (workItem == null)
                return NotFound(new { message = "Work item bulunamadı" });

            // Check if user has access to this work item's project
            var hasAccess = await CheckProjectAccess(workItem.ProjectId);
            if (!hasAccess)
                return Forbid();

            return Ok(await MapToResponseDto(workItem));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Work item alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<WorkItemResponseDto>>> GetWorkItemsByProject(string projectId)
    {
        try
        {
            // Check if user has access to this project
            var hasAccess = await CheckProjectAccess(projectId);
            if (!hasAccess)
                return Forbid();

            var workItems = await _workItemService.GetWorkItemsByProjectIdAsync(projectId);
            var workItemDtos = new List<WorkItemResponseDto>();

            foreach (var workItem in workItems)
            {
                workItemDtos.Add(await MapToResponseDto(workItem));
            }

            return Ok(workItemDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Proje work itemları alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<WorkItemResponseDto>>> GetMyWorkItems()
    {
        try
        {
            var userId = GetCurrentUserId();
            var workItems = await _workItemService.GetWorkItemsByAssigneeIdAsync(userId);
            var workItemDtos = new List<WorkItemResponseDto>();

            foreach (var workItem in workItems)
            {
                workItemDtos.Add(await MapToResponseDto(workItem));
            }

            return Ok(workItemDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Work itemlarım alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<IEnumerable<WorkItemResponseDto>>> GetOverdueWorkItems()
    {
        try
        {
            var workItems = await _workItemService.GetOverdueWorkItemsAsync();
            var workItemDtos = new List<WorkItemResponseDto>();

            foreach (var workItem in workItems)
            {
                // Check if user has access to this work item's project
                var hasAccess = await CheckProjectAccess(workItem.ProjectId);
                if (hasAccess)
                {
                    workItemDtos.Add(await MapToResponseDto(workItem));
                }
            }

            return Ok(workItemDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Geciken work itemlar alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("due-soon")]
    public async Task<ActionResult<IEnumerable<WorkItemResponseDto>>> GetWorkItemsDueSoon([FromQuery] int days = 3)
    {
        try
        {
            var workItems = await _workItemService.GetWorkItemsDueSoonAsync(days);
            var workItemDtos = new List<WorkItemResponseDto>();

            foreach (var workItem in workItems)
            {
                // Check if user has access to this work item's project
                var hasAccess = await CheckProjectAccess(workItem.ProjectId);
                if (hasAccess)
                {
                    workItemDtos.Add(await MapToResponseDto(workItem));
                }
            }

            return Ok(workItemDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Yakında bitecek work itemlar alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<WorkItemResponseDto>> CreateWorkItem(CreateWorkItemDto createWorkItemDto)
    {
        try
        {
            // Check if user has access to create work items in this project
            var hasAccess = await CheckProjectAccess(createWorkItemDto.ProjectId);
            if (!hasAccess)
                return Forbid();

            var userId = GetCurrentUserId();
            var workItem = await _workItemService.CreateWorkItemAsync(createWorkItemDto, userId);

            // Send notification if task is assigned during creation
            if (!string.IsNullOrEmpty(workItem.AssignedToId) && workItem.AssignedToId != userId)
            {
                await _notificationService.SendWorkItemAssignedNotificationAsync(workItem.Id, workItem.AssignedToId, userId);
            }

            return CreatedAtAction(nameof(GetWorkItem), new { id = workItem.Id }, await MapToResponseDto(workItem));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Work item oluşturulurken hata oluştu", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WorkItemResponseDto>> UpdateWorkItem(string id, UpdateWorkItemDto updateWorkItemDto)
    {
        try
        {
            var existingWorkItem = await _workItemService.GetWorkItemByIdAsync(id);
            if (existingWorkItem == null)
                return NotFound(new { message = "Work item bulunamadı" });

            // Check if user has access to this work item's project
            var hasAccess = await CheckProjectAccess(existingWorkItem.ProjectId);
            if (!hasAccess)
                return Forbid();

            var userId = GetCurrentUserId();
            var oldWorkItem = existingWorkItem; // Store old state for comparison
            var workItem = await _workItemService.UpdateWorkItemAsync(id, updateWorkItemDto, userId);

            if (workItem == null)
                return NotFound(new { message = "Work item bulunamadı" });

            // Send notification for work item update
            await _notificationService.SendWorkItemUpdatedNotificationAsync(workItem.Id, userId);

            // Send notification if assignee changed
            if (oldWorkItem.AssignedToId != workItem.AssignedToId && !string.IsNullOrEmpty(workItem.AssignedToId))
            {
                await _notificationService.SendWorkItemAssignedNotificationAsync(workItem.Id, workItem.AssignedToId, userId);
            }

            return Ok(await MapToResponseDto(workItem));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Work item güncellenirken hata oluştu", error = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateWorkItemStatus(string id, UpdateWorkItemStatusDto statusDto)
    {
        try
        {
            var workItem = await _workItemService.GetWorkItemByIdAsync(id);
            if (workItem == null)
                return NotFound(new { message = "Work item bulunamadı" });

            // Check if user has access to this work item's project
            var hasAccess = await CheckProjectAccess(workItem.ProjectId);
            if (!hasAccess)
                return Forbid();

            var userId = GetCurrentUserId();
            var success = await _workItemService.UpdateWorkItemStatusAsync(id, statusDto.Status, userId);

            if (!success)
                return BadRequest(new { message = "Work item durumu güncellenemedi" });

            // Send notification for status update
            await _notificationService.SendWorkItemUpdatedNotificationAsync(id, userId);

            // Send completion notification if status is Done
            if (statusDto.Status == WorkItemStatus.Done)
            {
                await _notificationService.SendWorkItemCompletedNotificationAsync(id, userId);
            }

            return Ok(new { message = "Work item durumu başarıyla güncellendi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Work item durumu güncellenirken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("{id}/logs")]
    public async Task<ActionResult<IEnumerable<WorkItemLogResponseDto>>> GetWorkItemLogs(string id)
    {
        try
        {
            var workItem = await _workItemService.GetWorkItemByIdAsync(id);
            if (workItem == null)
                return NotFound(new { message = "Work item bulunamadı" });

            // Check if user has access to this work item's project
            var hasAccess = await CheckProjectAccess(workItem.ProjectId);
            if (!hasAccess)
                return Forbid();

            var logs = await _workItemService.GetWorkItemLogsAsync(id);
            var logDtos = new List<WorkItemLogResponseDto>();

            foreach (var log in logs.OrderByDescending(l => l.CreatedAt))
            {
                var user = await _userRepository.GetByIdAsync(log.UserId);
                logDtos.Add(new WorkItemLogResponseDto
                {
                    Id = log.Id,
                    WorkItemId = log.WorkItemId,
                    UserId = log.UserId,
                    UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Bilinmeyen Kullanıcı",
                    Action = log.Action,
                    Description = log.Description,
                    OldValue = log.OldValue,
                    NewValue = log.NewValue,
                    FieldName = log.FieldName,
                    CreatedAt = log.CreatedAt,
                    Metadata = log.Metadata
                });
            }

            return Ok(logDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Work item logları alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpPatch("{id}/assign")]
    public async Task<ActionResult> AssignWorkItem(string id, AssignWorkItemDto assignDto)
    {
        try
        {
            var workItem = await _workItemService.GetWorkItemByIdAsync(id);
            if (workItem == null)
                return NotFound(new { message = "Work item bulunamadı" });

            // Check if user has access to this work item's project
            var hasAccess = await CheckProjectAccess(workItem.ProjectId);
            if (!hasAccess)
                return Forbid();

            var userId = GetCurrentUserId();
            var success = await _workItemService.AssignWorkItemAsync(id, assignDto.AssigneeId, userId);

            if (!success)
                return BadRequest(new { message = "Work item atanamadı" });

            // Send assignment notification
            if (assignDto.AssigneeId != userId)
            {
                await _notificationService.SendWorkItemAssignedNotificationAsync(id, assignDto.AssigneeId, userId);
            }

            return Ok(new { message = "Work item başarıyla atandı" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Work item atanırken hata oluştu", error = ex.Message });
        }
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult> AddComment(string id, AddWorkItemCommentDto commentDto)
    {
        try
        {
            var workItem = await _workItemService.GetWorkItemByIdAsync(id);
            if (workItem == null)
                return NotFound(new { message = "Work item bulunamadı" });

            // Check if user has access to this work item's project
            var hasAccess = await CheckProjectAccess(workItem.ProjectId);
            if (!hasAccess)
                return Forbid();

            var userId = GetCurrentUserId();
            var success = await _workItemService.AddCommentAsync(id, commentDto.Comment, userId);

            if (!success)
                return BadRequest(new { message = "Yorum eklenemedi" });

            return Ok(new { message = "Yorum başarıyla eklendi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Yorum eklenirken hata oluştu", error = ex.Message });
        }
    }



    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> DeleteWorkItem(string id)
    {
        try
        {
            var workItem = await _workItemService.GetWorkItemByIdAsync(id);
            if (workItem == null)
                return NotFound(new { message = "Work item bulunamadı" });

            // Check if user has access to this work item's project
            var hasAccess = await CheckProjectAccess(workItem.ProjectId);
            if (!hasAccess)
                return Forbid();

            var userId = GetCurrentUserId();
            var success = await _workItemService.DeleteWorkItemAsync(id, userId);

            if (!success)
                return BadRequest(new { message = "Work item silinemedi" });

            return Ok(new { message = "Work item başarıyla silindi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Work item silinirken hata oluştu", error = ex.Message });
        }
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
               User.FindFirst("userId")?.Value ?? 
               throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı");
    }

    private async Task<bool> CheckProjectAccess(string projectId)
    {
        var userId = GetCurrentUserId();
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Admin has access to all projects
        if (userRole == "Admin")
            return true;

        // Check if user is in project
        return await _projectService.IsUserInProjectAsync(projectId, userId);
    }

    private async Task<WorkItemResponseDto> MapToResponseDto(WorkItem workItem)
    {
        var dto = new WorkItemResponseDto
        {
            Id = workItem.Id,
            Title = workItem.Title,
            Description = workItem.Description,
            ProjectId = workItem.ProjectId,
            AssignedToId = workItem.AssignedToId,
            CreatedById = workItem.CreatedById,
            Status = workItem.Status,
            Priority = workItem.Priority,
            CreatedAt = workItem.CreatedAt,
            UpdatedAt = workItem.UpdatedAt,
            DueDate = workItem.DueDate,
            CompletedAt = workItem.CompletedAt,
            EstimatedHours = workItem.EstimatedHours,
            ActualHours = workItem.ActualHours,
            Tags = workItem.Tags,
            Attachments = workItem.Attachments,
            Comments = workItem.Comments.Select(c => new WorkItemCommentDto
            {
                Id = c.Id,
                UserId = c.UserId,
                Comment = c.Comment,
                CreatedAt = c.CreatedAt
            }).ToList()
        };

        // Get user names
        if (!string.IsNullOrEmpty(workItem.AssignedToId))
        {
            var assignee = await _userRepository.GetByIdAsync(workItem.AssignedToId);
            dto.AssignedToName = assignee != null ? $"{assignee.FirstName} {assignee.LastName}" : null;
        }

        var creator = await _userRepository.GetByIdAsync(workItem.CreatedById);
        dto.CreatedByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : null;

        var project = await _projectService.GetProjectByIdAsync(workItem.ProjectId);
        dto.ProjectName = project?.Name;

        // Get comment user names
        foreach (var comment in dto.Comments)
        {
            var user = await _userRepository.GetByIdAsync(comment.UserId);
            comment.UserName = user != null ? $"{user.FirstName} {user.LastName}" : null;
        }

        return dto;
    }
}

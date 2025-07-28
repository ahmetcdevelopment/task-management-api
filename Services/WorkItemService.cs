using TaskManagement.API.DTOs;
using TaskManagement.API.Models;
using TaskManagement.API.Repositories;

namespace TaskManagement.API.Services;

public class WorkItemService : IWorkItemService
{
    private readonly IWorkItemRepository _workItemRepository;
    private readonly IWorkItemLogRepository _workItemLogRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public WorkItemService(
        IWorkItemRepository workItemRepository,
        IWorkItemLogRepository workItemLogRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _workItemRepository = workItemRepository;
        _workItemLogRepository = workItemLogRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<WorkItem>> GetAllWorkItemsAsync()
    {
        return await _workItemRepository.GetAllAsync();
    }

    public async Task<WorkItem?> GetWorkItemByIdAsync(string id)
    {
        return await _workItemRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<WorkItem>> GetWorkItemsByProjectIdAsync(string projectId)
    {
        return await _workItemRepository.GetByProjectIdAsync(projectId);
    }

    public async Task<IEnumerable<WorkItem>> GetWorkItemsByAssigneeIdAsync(string assigneeId)
    {
        return await _workItemRepository.GetByAssigneeIdAsync(assigneeId);
    }

    public async Task<IEnumerable<WorkItem>> GetWorkItemsByStatusAsync(WorkItemStatus status)
    {
        return await _workItemRepository.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<WorkItem>> GetOverdueWorkItemsAsync()
    {
        return await _workItemRepository.GetOverdueWorkItemsAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetWorkItemsDueSoonAsync(int days = 3)
    {
        return await _workItemRepository.GetWorkItemsDueSoonAsync(days);
    }

    public async Task<WorkItem> CreateWorkItemAsync(CreateWorkItemDto createWorkItemDto, string createdById)
    {
        // Validate project exists
        var project = await _projectRepository.GetByIdAsync(createWorkItemDto.ProjectId);
        if (project == null)
        {
            throw new ArgumentException("Project not found");
        }

        // Validate assignee exists if provided
        if (!string.IsNullOrEmpty(createWorkItemDto.AssignedToId))
        {
            var assignee = await _userRepository.GetByIdAsync(createWorkItemDto.AssignedToId);
            if (assignee == null)
            {
                throw new ArgumentException("Assignee not found");
            }
        }

        var workItem = new WorkItem
        {
            Title = createWorkItemDto.Title,
            Description = createWorkItemDto.Description,
            ProjectId = createWorkItemDto.ProjectId,
            AssignedToId = createWorkItemDto.AssignedToId,
            CreatedById = createdById,
            Priority = createWorkItemDto.Priority,
            DueDate = createWorkItemDto.DueDate,
            EstimatedHours = createWorkItemDto.EstimatedHours,
            Tags = createWorkItemDto.Tags,
            Status = WorkItemStatus.ToDo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdWorkItem = await _workItemRepository.CreateAsync(workItem);

        // Log the creation
        await LogWorkItemActionAsync(createdWorkItem.Id, createdById, WorkItemLogAction.Created, "WorkItem created");

        // Send notification if assigned
        if (!string.IsNullOrEmpty(createWorkItemDto.AssignedToId))
        {
            await _notificationService.SendWorkItemAssignedNotificationAsync(createdWorkItem.Id, createWorkItemDto.AssignedToId, createdById);
        }

        return createdWorkItem;
    }

    public async Task<WorkItem?> UpdateWorkItemAsync(string id, UpdateWorkItemDto updateWorkItemDto, string updatedById)
    {
        var existingWorkItem = await _workItemRepository.GetByIdAsync(id);
        if (existingWorkItem == null)
        {
            return null;
        }

        // Track changes for logging
        var changes = new List<string>();

        if (!string.IsNullOrEmpty(updateWorkItemDto.Title) && updateWorkItemDto.Title != existingWorkItem.Title)
        {
            changes.Add($"Title changed from '{existingWorkItem.Title}' to '{updateWorkItemDto.Title}'");
            existingWorkItem.Title = updateWorkItemDto.Title;
        }

        if (!string.IsNullOrEmpty(updateWorkItemDto.Description) && updateWorkItemDto.Description != existingWorkItem.Description)
        {
            changes.Add($"Description updated");
            existingWorkItem.Description = updateWorkItemDto.Description;
        }

        if (updateWorkItemDto.AssignedToId != null && updateWorkItemDto.AssignedToId != existingWorkItem.AssignedToId)
        {
            // Validate new assignee exists
            if (!string.IsNullOrEmpty(updateWorkItemDto.AssignedToId))
            {
                var assignee = await _userRepository.GetByIdAsync(updateWorkItemDto.AssignedToId);
                if (assignee == null)
                {
                    throw new ArgumentException("Assignee not found");
                }
            }

            var oldAssignee = existingWorkItem.AssignedToId ?? "Unassigned";
            var newAssignee = updateWorkItemDto.AssignedToId ?? "Unassigned";
            changes.Add($"Assignee changed from '{oldAssignee}' to '{newAssignee}'");
            existingWorkItem.AssignedToId = updateWorkItemDto.AssignedToId;
        }

        if (updateWorkItemDto.Priority.HasValue && updateWorkItemDto.Priority != existingWorkItem.Priority)
        {
            changes.Add($"Priority changed from '{existingWorkItem.Priority}' to '{updateWorkItemDto.Priority}'");
            existingWorkItem.Priority = updateWorkItemDto.Priority.Value;
        }

        if (updateWorkItemDto.DueDate != existingWorkItem.DueDate)
        {
            changes.Add($"Due date changed");
            existingWorkItem.DueDate = updateWorkItemDto.DueDate;
        }

        if (updateWorkItemDto.EstimatedHours.HasValue && updateWorkItemDto.EstimatedHours != existingWorkItem.EstimatedHours)
        {
            changes.Add($"Estimated hours changed from {existingWorkItem.EstimatedHours} to {updateWorkItemDto.EstimatedHours}");
            existingWorkItem.EstimatedHours = updateWorkItemDto.EstimatedHours.Value;
        }

        if (updateWorkItemDto.ActualHours.HasValue && updateWorkItemDto.ActualHours != existingWorkItem.ActualHours)
        {
            changes.Add($"Actual hours changed from {existingWorkItem.ActualHours} to {updateWorkItemDto.ActualHours}");
            existingWorkItem.ActualHours = updateWorkItemDto.ActualHours.Value;
        }

        if (updateWorkItemDto.Tags != null)
        {
            existingWorkItem.Tags = updateWorkItemDto.Tags;
            changes.Add("Tags updated");
        }

        if (changes.Any())
        {
            existingWorkItem.UpdatedAt = DateTime.UtcNow;
            var success = await _workItemRepository.UpdateAsync(id, existingWorkItem);

            if (success)
            {
                // Log the update
                await LogWorkItemActionAsync(id, updatedById, WorkItemLogAction.Updated, string.Join(", ", changes));

                // Send notification
                await _notificationService.SendWorkItemUpdatedNotificationAsync(id, updatedById);
            }
        }

        return existingWorkItem;
    }

    public async Task<bool> DeleteWorkItemAsync(string id, string deletedById)
    {
        var workItem = await _workItemRepository.GetByIdAsync(id);
        if (workItem == null)
        {
            return false;
        }

        // Log the deletion before deleting
        await LogWorkItemActionAsync(id, deletedById, WorkItemLogAction.Deleted, "WorkItem deleted");

        var success = await _workItemRepository.DeleteAsync(id);
        return success;
    }

    public async Task<bool> AssignWorkItemAsync(string workItemId, string assigneeId, string assignedById)
    {
        var workItem = await _workItemRepository.GetByIdAsync(workItemId);
        if (workItem == null)
        {
            return false;
        }

        // Validate assignee exists
        var assignee = await _userRepository.GetByIdAsync(assigneeId);
        if (assignee == null)
        {
            return false;
        }

        var oldAssignee = workItem.AssignedToId ?? "Unassigned";
        workItem.AssignedToId = assigneeId;
        workItem.UpdatedAt = DateTime.UtcNow;

        var success = await _workItemRepository.UpdateAsync(workItemId, workItem);
        if (success)
        {
            // Log the assignment
            await LogWorkItemActionAsync(workItemId, assignedById, WorkItemLogAction.AssigneeChanged, 
                $"Assignee changed from '{oldAssignee}' to '{assignee.FirstName} {assignee.LastName}'");

            // Send notification
            await _notificationService.SendWorkItemAssignedNotificationAsync(workItemId, assigneeId, assignedById);
        }

        return success;
    }

    public async Task<bool> UpdateWorkItemStatusAsync(string workItemId, WorkItemStatus status, string updatedById)
    {
        var workItem = await _workItemRepository.GetByIdAsync(workItemId);
        if (workItem == null)
        {
            return false;
        }

        var oldStatus = workItem.Status;
        workItem.Status = status;
        workItem.UpdatedAt = DateTime.UtcNow;

        if (status == WorkItemStatus.Done)
        {
            workItem.CompletedAt = DateTime.UtcNow;
        }

        var success = await _workItemRepository.UpdateAsync(workItemId, workItem);
        if (success)
        {
            // Log the status change
            await LogWorkItemActionAsync(workItemId, updatedById, WorkItemLogAction.StatusChanged, 
                $"Status changed from '{oldStatus}' to '{status}'");

            // Send notification if completed
            if (status == WorkItemStatus.Done)
            {
                await _notificationService.SendWorkItemCompletedNotificationAsync(workItemId, updatedById);
            }
            else
            {
                await _notificationService.SendWorkItemUpdatedNotificationAsync(workItemId, updatedById);
            }
        }

        return success;
    }

    public async Task<bool> AddCommentAsync(string workItemId, string comment, string userId)
    {
        var workItem = await _workItemRepository.GetByIdAsync(workItemId);
        if (workItem == null)
        {
            return false;
        }

        var workItemComment = new WorkItemComment
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };

        workItem.Comments.Add(workItemComment);
        workItem.UpdatedAt = DateTime.UtcNow;

        var success = await _workItemRepository.UpdateAsync(workItemId, workItem);
        if (success)
        {
            // Log the comment
            await LogWorkItemActionAsync(workItemId, userId, WorkItemLogAction.CommentAdded, "Comment added");
        }

        return success;
    }

    public async Task<IEnumerable<WorkItemLog>> GetWorkItemLogsAsync(string workItemId)
    {
        return await _workItemLogRepository.GetByWorkItemIdAsync(workItemId);
    }

    private async Task LogWorkItemActionAsync(string workItemId, string userId, WorkItemLogAction action, string description)
    {
        var log = new WorkItemLog
        {
            WorkItemId = workItemId,
            UserId = userId,
            Action = action,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await _workItemLogRepository.CreateAsync(log);
    }
}

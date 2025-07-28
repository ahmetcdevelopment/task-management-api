using TaskManagement.API.Models;
using TaskManagement.API.DTOs;
using TaskManagement.API.Repositories;

namespace TaskManagement.API.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IWorkItemRepository _workItemRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public NotificationService(
        INotificationRepository notificationRepository,
        IWorkItemRepository workItemRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository)
    {
        _notificationRepository = notificationRepository;
        _workItemRepository = workItemRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
    {
        return await _notificationRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId)
    {
        return await _notificationRepository.GetUnreadByUserIdAsync(userId);
    }

    public async Task<long> GetUnreadCountAsync(string userId)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId);
    }

    public async Task<Notification> CreateNotificationAsync(CreateNotificationDto createNotificationDto)
    {
        var notification = new Notification
        {
            UserId = createNotificationDto.UserId,
            Title = createNotificationDto.Title,
            Message = createNotificationDto.Message,
            Type = (Models.NotificationType)createNotificationDto.Type,
            RelatedEntityId = createNotificationDto.RelatedEntityId,
            RelatedEntityType = createNotificationDto.RelatedEntityType,
            ActionUrl = createNotificationDto.ActionUrl,
            Metadata = createNotificationDto.Metadata ?? new Dictionary<string, object>()
        };

        return await _notificationRepository.CreateAsync(notification);
    }

    public async Task<bool> MarkAsReadAsync(string notificationId)
    {
        return await _notificationRepository.MarkAsReadAsync(notificationId);
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        return await _notificationRepository.MarkAllAsReadAsync(userId);
    }

    public async Task<bool> DeleteNotificationAsync(string notificationId)
    {
        return await _notificationRepository.DeleteAsync(notificationId);
    }

    public async Task<bool> DeleteOldNotificationsAsync(int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        return await _notificationRepository.DeleteOldNotificationsAsync(cutoffDate);
    }

    public async Task SendWorkItemAssignedNotificationAsync(string workItemId, string assigneeId, string assignedById)
    {
        var workItem = await _workItemRepository.GetByIdAsync(workItemId);
        var assignedBy = await _userRepository.GetByIdAsync(assignedById);
        
        if (workItem == null || assignedBy == null)
            return;

        var notification = new CreateNotificationDto
        {
            UserId = assigneeId,
            Title = "New WorkItem Assigned",
            Message = $"You have been assigned to work item: {workItem.Title} by {assignedBy.FirstName} {assignedBy.LastName}",
            Type = DTOs.NotificationType.TaskAssigned,
            RelatedEntityId = workItemId,
            RelatedEntityType = "WorkItem",
            ActionUrl = $"/workitems/{workItemId}",
            Metadata = new Dictionary<string, object>
            {
                { "workItemId", workItemId },
                { "assignedById", assignedById },
                { "projectId", workItem.ProjectId }
            }
        };

        await CreateNotificationAsync(notification);
    }

    public async Task SendWorkItemUpdatedNotificationAsync(string workItemId, string updatedById)
    {
        var workItem = await _workItemRepository.GetByIdAsync(workItemId);
        var updatedBy = await _userRepository.GetByIdAsync(updatedById);
        
        if (workItem == null || updatedBy == null)
            return;

        // Notify assignee if different from updater
        if (!string.IsNullOrEmpty(workItem.AssignedToId) && workItem.AssignedToId != updatedById)
        {
            var notification = new CreateNotificationDto
            {
                UserId = workItem.AssignedToId,
                Title = "WorkItem Updated",
                Message = $"Work item '{workItem.Title}' has been updated by {updatedBy.FirstName} {updatedBy.LastName}",
                Type = DTOs.NotificationType.TaskUpdated,
                RelatedEntityId = workItemId,
                RelatedEntityType = "WorkItem",
                ActionUrl = $"/workitems/{workItemId}",
                Metadata = new Dictionary<string, object>
                {
                    { "workItemId", workItemId },
                    { "updatedById", updatedById },
                    { "projectId", workItem.ProjectId }
                }
            };

            await CreateNotificationAsync(notification);
        }
    }

    public async Task SendWorkItemCompletedNotificationAsync(string workItemId, string completedById)
    {
        var workItem = await _workItemRepository.GetByIdAsync(workItemId);
        var completedBy = await _userRepository.GetByIdAsync(completedById);
        var project = await _projectRepository.GetByIdAsync(workItem?.ProjectId ?? "");
        
        if (workItem == null || completedBy == null || project == null)
            return;

        // Notify project manager
        if (project.ManagerId != completedById)
        {
            var notification = new CreateNotificationDto
            {
                UserId = project.ManagerId,
                Title = "WorkItem Completed",
                Message = $"Work item '{workItem.Title}' has been completed by {completedBy.FirstName} {completedBy.LastName}",
                Type = DTOs.NotificationType.TaskCompleted,
                RelatedEntityId = workItemId,
                RelatedEntityType = "WorkItem",
                ActionUrl = $"/workitems/{workItemId}",
                Metadata = new Dictionary<string, object>
                {
                    { "workItemId", workItemId },
                    { "completedById", completedById },
                    { "projectId", workItem.ProjectId }
                }
            };

            await CreateNotificationAsync(notification);
        }
    }

    public async Task SendProjectCreatedNotificationAsync(string projectId, string createdById)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        var createdBy = await _userRepository.GetByIdAsync(createdById);
        
        if (project == null || createdBy == null)
            return;

        // Notify all team members
        foreach (var teamMemberId in project.TeamMemberIds)
        {
            if (teamMemberId != createdById)
            {
                var notification = new CreateNotificationDto
                {
                    UserId = teamMemberId,
                    Title = "Added to New Project",
                    Message = $"You have been added to project '{project.Name}' by {createdBy.FirstName} {createdBy.LastName}",
                    Type = DTOs.NotificationType.ProjectCreated,
                    RelatedEntityId = projectId,
                    RelatedEntityType = "Project",
                    ActionUrl = $"/projects/{projectId}",
                    Metadata = new Dictionary<string, object>
                    {
                        { "projectId", projectId },
                        { "createdById", createdById }
                    }
                };

                await CreateNotificationAsync(notification);
            }
        }

        // Notify project manager if different from creator
        if (project.ManagerId != createdById)
        {
            var notification = new CreateNotificationDto
            {
                UserId = project.ManagerId,
                Title = "Assigned as Project Manager",
                Message = $"You have been assigned as manager for project '{project.Name}'",
                Type = DTOs.NotificationType.ProjectCreated,
                RelatedEntityId = projectId,
                RelatedEntityType = "Project",
                ActionUrl = $"/projects/{projectId}",
                Metadata = new Dictionary<string, object>
                {
                    { "projectId", projectId },
                    { "createdById", createdById }
                }
            };

            await CreateNotificationAsync(notification);
        }
    }

    public async Task SendProjectUpdatedNotificationAsync(string projectId, string updatedById)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        var updatedBy = await _userRepository.GetByIdAsync(updatedById);
        
        if (project == null || updatedBy == null)
            return;

        // Notify all team members except updater
        foreach (var teamMemberId in project.TeamMemberIds)
        {
            if (teamMemberId != updatedById)
            {
                var notification = new CreateNotificationDto
                {
                    UserId = teamMemberId,
                    Title = "Project Updated",
                    Message = $"Project '{project.Name}' has been updated by {updatedBy.FirstName} {updatedBy.LastName}",
                    Type = DTOs.NotificationType.ProjectUpdated,
                    RelatedEntityId = projectId,
                    RelatedEntityType = "Project",
                    ActionUrl = $"/projects/{projectId}",
                    Metadata = new Dictionary<string, object>
                    {
                        { "projectId", projectId },
                        { "updatedById", updatedById }
                    }
                };

                await CreateNotificationAsync(notification);
            }
        }

        // Notify project manager if different from updater
        if (project.ManagerId != updatedById && !project.TeamMemberIds.Contains(project.ManagerId))
        {
            var notification = new CreateNotificationDto
            {
                UserId = project.ManagerId,
                Title = "Project Updated",
                Message = $"Project '{project.Name}' has been updated by {updatedBy.FirstName} {updatedBy.LastName}",
                Type = DTOs.NotificationType.ProjectUpdated,
                RelatedEntityId = projectId,
                RelatedEntityType = "Project",
                ActionUrl = $"/projects/{projectId}",
                Metadata = new Dictionary<string, object>
                {
                    { "projectId", projectId },
                    { "updatedById", updatedById }
                }
            };

            await CreateNotificationAsync(notification);
        }
    }

    public async Task SendTeamMemberAddedNotificationAsync(string projectId, string userId, string addedById)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        var addedBy = await _userRepository.GetByIdAsync(addedById);
        
        if (project == null || addedBy == null)
            return;

        var notification = new CreateNotificationDto
        {
            UserId = userId,
            Title = "Added to Project",
            Message = $"You have been added to project '{project.Name}' by {addedBy.FirstName} {addedBy.LastName}",
            Type = DTOs.NotificationType.TeamMemberAdded,
            RelatedEntityId = projectId,
            RelatedEntityType = "Project",
            ActionUrl = $"/projects/{projectId}",
            Metadata = new Dictionary<string, object>
            {
                { "projectId", projectId },
                { "addedById", addedById }
            }
        };

        await CreateNotificationAsync(notification);
    }

    public async Task SendTeamMemberRemovedNotificationAsync(string projectId, string userId, string removedById)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        var removedBy = await _userRepository.GetByIdAsync(removedById);
        
        if (project == null || removedBy == null)
            return;

        var notification = new CreateNotificationDto
        {
            UserId = userId,
            Title = "Removed from Project",
            Message = $"You have been removed from project '{project.Name}' by {removedBy.FirstName} {removedBy.LastName}",
            Type = DTOs.NotificationType.TeamMemberRemoved,
            RelatedEntityId = projectId,
            RelatedEntityType = "Project",
            ActionUrl = $"/projects",
            Metadata = new Dictionary<string, object>
            {
                { "projectId", projectId },
                { "removedById", removedById }
            }
        };

        await CreateNotificationAsync(notification);
    }
}
using TaskManagement.API.Models;
using TaskManagement.API.DTOs;

namespace TaskManagement.API.Services;

public interface INotificationService
{
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId);
    Task<long> GetUnreadCountAsync(string userId);
    Task<Notification> CreateNotificationAsync(CreateNotificationDto createNotificationDto);
    Task<bool> MarkAsReadAsync(string notificationId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<bool> DeleteNotificationAsync(string notificationId);
    Task<bool> DeleteOldNotificationsAsync(int daysOld = 30);
    Task SendWorkItemAssignedNotificationAsync(string workItemId, string assigneeId, string assignedById);
    Task SendWorkItemUpdatedNotificationAsync(string workItemId, string updatedById);
    Task SendWorkItemCompletedNotificationAsync(string workItemId, string completedById);
    Task SendProjectCreatedNotificationAsync(string projectId, string createdById);
    Task SendProjectUpdatedNotificationAsync(string projectId, string updatedById);
    Task SendTeamMemberAddedNotificationAsync(string projectId, string userId, string addedById);
    Task SendTeamMemberRemovedNotificationAsync(string projectId, string userId, string removedById);
}
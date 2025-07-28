using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

public interface INotificationRepository : IBaseRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId);
    Task<bool> MarkAsReadAsync(string notificationId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<IEnumerable<Notification>> GetByTypeAsync(NotificationType type);
    Task<long> GetUnreadCountAsync(string userId);
    Task<bool> DeleteOldNotificationsAsync(DateTime beforeDate);
}
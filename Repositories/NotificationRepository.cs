using MongoDB.Driver;
using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(IMongoDatabase database) : base(database, "notifications")
    {
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(n => n.UserId == userId)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId)
    {
        return await _collection.Find(n => n.UserId == userId && !n.IsRead)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> MarkAsReadAsync(string notificationId)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
        var update = Builders<Notification>.Update
            .Set(n => n.IsRead, true)
            .Set(n => n.ReadAt, DateTime.UtcNow);
        
        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        var filter = Builders<Notification>.Filter.And(
            Builders<Notification>.Filter.Eq(n => n.UserId, userId),
            Builders<Notification>.Filter.Eq(n => n.IsRead, false)
        );
        
        var update = Builders<Notification>.Update
            .Set(n => n.IsRead, true)
            .Set(n => n.ReadAt, DateTime.UtcNow);
        
        var result = await _collection.UpdateManyAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<IEnumerable<Notification>> GetByTypeAsync(NotificationType type)
    {
        return await _collection.Find(n => n.Type == type)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<long> GetUnreadCountAsync(string userId)
    {
        return await _collection.CountDocumentsAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<bool> DeleteOldNotificationsAsync(DateTime beforeDate)
    {
        var filter = Builders<Notification>.Filter.Lt(n => n.CreatedAt, beforeDate);
        var result = await _collection.DeleteManyAsync(filter);
        return result.DeletedCount > 0;
    }
}
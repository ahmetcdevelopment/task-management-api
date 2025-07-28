using MongoDB.Driver;
using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

public class WorkItemLogRepository : BaseRepository<WorkItemLog>, IWorkItemLogRepository
{
    public WorkItemLogRepository(IMongoDatabase database) : base(database, "workitemlogs")
    {
    }

    public async Task<IEnumerable<WorkItemLog>> GetByWorkItemIdAsync(string workItemId)
    {
        var filter = Builders<WorkItemLog>.Filter.Eq(log => log.WorkItemId, workItemId);
        var sort = Builders<WorkItemLog>.Sort.Descending(log => log.CreatedAt);
        return await _collection.Find(filter).Sort(sort).ToListAsync();
    }

    public async Task<IEnumerable<WorkItemLog>> GetByUserIdAsync(string userId)
    {
        var filter = Builders<WorkItemLog>.Filter.Eq(log => log.UserId, userId);
        var sort = Builders<WorkItemLog>.Sort.Descending(log => log.CreatedAt);
        return await _collection.Find(filter).Sort(sort).ToListAsync();
    }

    public async Task<IEnumerable<WorkItemLog>> GetByActionAsync(WorkItemLogAction action)
    {
        var filter = Builders<WorkItemLog>.Filter.Eq(log => log.Action, action);
        var sort = Builders<WorkItemLog>.Sort.Descending(log => log.CreatedAt);
        return await _collection.Find(filter).Sort(sort).ToListAsync();
    }

    public async Task<IEnumerable<WorkItemLog>> GetRecentLogsAsync(int count = 50)
    {
        var sort = Builders<WorkItemLog>.Sort.Descending(log => log.CreatedAt);
        return await _collection.Find(Builders<WorkItemLog>.Filter.Empty)
            .Sort(sort)
            .Limit(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkItemLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var filter = Builders<WorkItemLog>.Filter.And(
            Builders<WorkItemLog>.Filter.Gte(log => log.CreatedAt, startDate),
            Builders<WorkItemLog>.Filter.Lte(log => log.CreatedAt, endDate)
        );
        var sort = Builders<WorkItemLog>.Sort.Descending(log => log.CreatedAt);
        return await _collection.Find(filter).Sort(sort).ToListAsync();
    }
}

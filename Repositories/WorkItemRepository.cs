using MongoDB.Driver;
using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

public class WorkItemRepository : BaseRepository<WorkItem>, IWorkItemRepository
{
    public WorkItemRepository(IMongoDatabase database) : base(database, "workitems")
    {
    }

    public async Task<IEnumerable<WorkItem>> GetByProjectIdAsync(string projectId)
    {
        var filter = Builders<WorkItem>.Filter.Eq(w => w.ProjectId, projectId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetByAssigneeIdAsync(string assigneeId)
    {
        var filter = Builders<WorkItem>.Filter.Eq(w => w.AssignedToId, assigneeId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetByStatusAsync(WorkItemStatus status)
    {
        var filter = Builders<WorkItem>.Filter.Eq(w => w.Status, status);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetByPriorityAsync(WorkItemPriority priority)
    {
        var filter = Builders<WorkItem>.Filter.Eq(w => w.Priority, priority);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetOverdueWorkItemsAsync()
    {
        var filter = Builders<WorkItem>.Filter.And(
            Builders<WorkItem>.Filter.Lt(w => w.DueDate, DateTime.UtcNow),
            Builders<WorkItem>.Filter.Ne(w => w.Status, WorkItemStatus.Done),
            Builders<WorkItem>.Filter.Ne(w => w.Status, WorkItemStatus.Cancelled)
        );
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetWorkItemsDueSoonAsync(int days = 3)
    {
        var dueDate = DateTime.UtcNow.AddDays(days);
        var filter = Builders<WorkItem>.Filter.And(
            Builders<WorkItem>.Filter.Lte(w => w.DueDate, dueDate),
            Builders<WorkItem>.Filter.Gte(w => w.DueDate, DateTime.UtcNow),
            Builders<WorkItem>.Filter.Ne(w => w.Status, WorkItemStatus.Done),
            Builders<WorkItem>.Filter.Ne(w => w.Status, WorkItemStatus.Cancelled)
        );
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetByCreatedByIdAsync(string createdById)
    {
        var filter = Builders<WorkItem>.Filter.Eq(w => w.CreatedById, createdById);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var filter = Builders<WorkItem>.Filter.And(
            Builders<WorkItem>.Filter.Gte(w => w.CreatedAt, startDate),
            Builders<WorkItem>.Filter.Lte(w => w.CreatedAt, endDate)
        );
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetByTagsAsync(List<string> tags)
    {
        var filter = Builders<WorkItem>.Filter.AnyIn(w => w.Tags, tags);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> SearchWorkItemsAsync(string searchTerm)
    {
        var filter = Builders<WorkItem>.Filter.Or(
            Builders<WorkItem>.Filter.Regex(w => w.Title, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
            Builders<WorkItem>.Filter.Regex(w => w.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
        );
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<long> GetWorkItemCountByProjectAsync(string projectId)
    {
        var filter = Builders<WorkItem>.Filter.Eq(w => w.ProjectId, projectId);
        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<long> GetWorkItemCountByStatusAsync(WorkItemStatus status)
    {
        var filter = Builders<WorkItem>.Filter.Eq(w => w.Status, status);
        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<long> GetOverdueWorkItemCountAsync()
    {
        var filter = Builders<WorkItem>.Filter.And(
            Builders<WorkItem>.Filter.Lt(w => w.DueDate, DateTime.UtcNow),
            Builders<WorkItem>.Filter.Ne(w => w.Status, WorkItemStatus.Done),
            Builders<WorkItem>.Filter.Ne(w => w.Status, WorkItemStatus.Cancelled)
        );
        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<Dictionary<WorkItemStatus, long>> GetWorkItemCountsByStatusAsync()
    {
        var pipeline = new[]
        {
            new MongoDB.Bson.BsonDocument("$group", new MongoDB.Bson.BsonDocument
            {
                { "_id", "$Status" },
                { "count", new MongoDB.Bson.BsonDocument("$sum", 1) }
            })
        };

        var results = await _collection.AggregateAsync<MongoDB.Bson.BsonDocument>(pipeline);
        var counts = new Dictionary<WorkItemStatus, long>();

        await results.ForEachAsync(doc =>
        {
            var status = (WorkItemStatus)doc["_id"].AsInt32;
            var count = doc["count"].AsInt64;
            counts[status] = count;
        });

        return counts;
    }

    public async Task<Dictionary<WorkItemPriority, long>> GetWorkItemCountsByPriorityAsync()
    {
        var pipeline = new[]
        {
            new MongoDB.Bson.BsonDocument("$group", new MongoDB.Bson.BsonDocument
            {
                { "_id", "$Priority" },
                { "count", new MongoDB.Bson.BsonDocument("$sum", 1) }
            })
        };

        var results = await _collection.AggregateAsync<MongoDB.Bson.BsonDocument>(pipeline);
        var counts = new Dictionary<WorkItemPriority, long>();

        await results.ForEachAsync(doc =>
        {
            var priority = (WorkItemPriority)doc["_id"].AsInt32;
            var count = doc["count"].AsInt64;
            counts[priority] = count;
        });

        return counts;
    }
}

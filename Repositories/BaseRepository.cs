using MongoDB.Driver;
using System.Linq.Expressions;

namespace TaskManagement.API.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;

    public BaseRepository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).ToListAsync();
    }

    public async Task<T> CreateAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<bool> UpdateAsync(string id, T entity)
    {
        var filter = Builders<T>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));
        var result = await _collection.ReplaceOneAsync(filter, entity);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));
        var result = await _collection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<long> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _collection.CountDocumentsAsync(_ => true);
        
        return await _collection.CountDocumentsAsync(predicate);
    }
}

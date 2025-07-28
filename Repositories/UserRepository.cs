using MongoDB.Driver;
using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(IMongoDatabase database) : base(database, "users")
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
    {
        return await _collection.Find(u => u.Role == role && u.IsActive)
            .SortBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _collection.Find(u => u.IsActive)
            .SortBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var count = await _collection.CountDocumentsAsync(u => u.Email == email);
        return count > 0;
    }

    public async Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<string> userIds)
    {
        var filter = Builders<User>.Filter.In(u => u.Id, userIds);
        return await _collection.Find(filter)
            .SortBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();
    }
}
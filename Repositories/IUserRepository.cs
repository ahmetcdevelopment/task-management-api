using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<string> userIds);
}
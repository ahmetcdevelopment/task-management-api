using MongoDB.Driver;
using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

public class ProjectRepository : BaseRepository<Project>, IProjectRepository
{
    public ProjectRepository(IMongoDatabase database) : base(database, "projects")
    {
    }

    public async Task<IEnumerable<Project>> GetByManagerIdAsync(string managerId)
    {
        return await _collection.Find(p => p.ManagerId == managerId)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByTeamMemberIdAsync(string userId)
    {
        return await _collection.Find(p => p.TeamMemberIds.Contains(userId) || p.ManagerId == userId)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status)
    {
        return await _collection.Find(p => p.Status == status)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
    {
        var activeStatuses = new[] { ProjectStatus.Planning, ProjectStatus.InProgress };
        return await _collection.Find(p => activeStatuses.Contains(p.Status))
            .SortByDescending(p => p.Priority)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> IsUserInProjectAsync(string projectId, string userId)
    {
        var count = await _collection.CountDocumentsAsync(p => 
            p.Id == projectId && 
            (p.ManagerId == userId || p.TeamMemberIds.Contains(userId)));
        return count > 0;
    }
}
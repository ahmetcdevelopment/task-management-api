using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

public interface IProjectRepository : IBaseRepository<Project>
{
    Task<IEnumerable<Project>> GetByManagerIdAsync(string managerId);
    Task<IEnumerable<Project>> GetByTeamMemberIdAsync(string userId);
    Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status);
    Task<IEnumerable<Project>> GetActiveProjectsAsync();
    Task<bool> IsUserInProjectAsync(string projectId, string userId);
}

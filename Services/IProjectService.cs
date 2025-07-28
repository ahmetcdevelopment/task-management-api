using TaskManagement.API.Models;
using TaskManagement.API.DTOs;

namespace TaskManagement.API.Services;

public interface IProjectService
{
    Task<IEnumerable<Project>> GetAllProjectsAsync();
    Task<Project?> GetProjectByIdAsync(string id);
    Task<IEnumerable<Project>> GetProjectsByManagerIdAsync(string managerId);
    Task<IEnumerable<Project>> GetProjectsByTeamMemberIdAsync(string userId);
    Task<IEnumerable<Project>> GetActiveProjectsAsync();
    Task<IEnumerable<Project>> GetProjectsByStatusAsync(ProjectStatus status);
    Task<Project> CreateProjectAsync(CreateProjectDto createProjectDto, string createdById);
    Task<Project?> UpdateProjectAsync(string id, UpdateProjectDto updateProjectDto, string updatedById);
    Task<bool> DeleteProjectAsync(string id, string deletedById);
    Task<bool> AddTeamMemberAsync(string projectId, string userId, string addedById);
    Task<bool> RemoveTeamMemberAsync(string projectId, string userId, string removedById);
    Task<bool> UpdateProjectStatusAsync(string projectId, ProjectStatus status, string updatedById);
    Task<bool> IsUserInProjectAsync(string projectId, string userId);
}
using TaskManagement.API.Models;
using TaskManagement.API.DTOs;
using TaskManagement.API.Repositories;

namespace TaskManagement.API.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWorkItemRepository _workItemRepository;

    public ProjectService(
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IWorkItemRepository workItemRepository)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _workItemRepository = workItemRepository;
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        return await _projectRepository.GetAllAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(string id)
    {
        return await _projectRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Project>> GetProjectsByManagerIdAsync(string managerId)
    {
        return await _projectRepository.GetByManagerIdAsync(managerId);
    }

    public async Task<IEnumerable<Project>> GetProjectsByTeamMemberIdAsync(string userId)
    {
        return await _projectRepository.GetByTeamMemberIdAsync(userId);
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
    {
        return await _projectRepository.GetActiveProjectsAsync();
    }

    public async Task<IEnumerable<Project>> GetProjectsByStatusAsync(ProjectStatus status)
    {
        return await _projectRepository.GetByStatusAsync(status);
    }

    public async Task<Project> CreateProjectAsync(CreateProjectDto createProjectDto, string createdById)
    {
        // Validate manager exists
        var manager = await _userRepository.GetByIdAsync(createProjectDto.ManagerId);
        if (manager == null)
            throw new ArgumentException("Manager not found");

        // Validate team members exist if provided
        if (createProjectDto.TeamMemberIds?.Any() == true)
        {
            var teamMembers = await _userRepository.GetUsersByIdsAsync(createProjectDto.TeamMemberIds);
            if (teamMembers.Count() != createProjectDto.TeamMemberIds.Count())
                throw new ArgumentException("One or more team members not found");
        }

        var project = new Project
        {
            Name = createProjectDto.Name,
            Description = createProjectDto.Description,
            ManagerId = createProjectDto.ManagerId,
            TeamMemberIds = createProjectDto.TeamMemberIds?.ToList() ?? new List<string>(),
            Status = ProjectStatus.Planning,
            StartDate = createProjectDto.StartDate,
            EndDate = createProjectDto.EndDate,
            Budget = createProjectDto.Budget,
            Priority = createProjectDto.Priority,
            Tags = createProjectDto.Tags?.ToList() ?? new List<string>()
        };

        return await _projectRepository.CreateAsync(project);
    }

    public async Task<Project?> UpdateProjectAsync(string id, UpdateProjectDto updateProjectDto, string updatedById)
    {
        var existingProject = await _projectRepository.GetByIdAsync(id);
        if (existingProject == null)
            return null;

        if (!string.IsNullOrEmpty(updateProjectDto.Name))
            existingProject.Name = updateProjectDto.Name;

        if (!string.IsNullOrEmpty(updateProjectDto.Description))
            existingProject.Description = updateProjectDto.Description;

        if (updateProjectDto.StartDate.HasValue)
            existingProject.StartDate = updateProjectDto.StartDate.Value;

        if (updateProjectDto.EndDate.HasValue)
            existingProject.EndDate = updateProjectDto.EndDate.Value;

        if (updateProjectDto.Budget.HasValue)
            existingProject.Budget = updateProjectDto.Budget.Value;

        if (updateProjectDto.Priority.HasValue)
            existingProject.Priority = updateProjectDto.Priority.Value;

        if (updateProjectDto.Tags != null)
            existingProject.Tags = updateProjectDto.Tags.ToList();

        existingProject.UpdatedAt = DateTime.UtcNow;

        var success = await _projectRepository.UpdateAsync(id, existingProject);
        return success ? existingProject : null;
    }

    public async Task<bool> DeleteProjectAsync(string id, string deletedById)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
            return false;

        // Check if project has active work items
        var projectWorkItems = await _workItemRepository.GetByProjectIdAsync(id);
        var activeWorkItems = projectWorkItems.Where(t => t.Status != WorkItemStatus.Done && t.Status != WorkItemStatus.Cancelled);
        
        if (activeWorkItems.Any())
            throw new InvalidOperationException("Cannot delete project with active work items");

        return await _projectRepository.DeleteAsync(id);
    }

    public async Task<bool> AddTeamMemberAsync(string projectId, string userId, string addedById)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            return false;

        // Validate user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        // Check if user is already in the project
        if (project.TeamMemberIds.Contains(userId) || project.ManagerId == userId)
            return false;

        project.TeamMemberIds.Add(userId);
        project.UpdatedAt = DateTime.UtcNow;

        return await _projectRepository.UpdateAsync(projectId, project);
    }

    public async Task<bool> RemoveTeamMemberAsync(string projectId, string userId, string removedById)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            return false;

        // Cannot remove the project manager
        if (project.ManagerId == userId)
            return false;

        if (!project.TeamMemberIds.Contains(userId))
            return false;

        project.TeamMemberIds.Remove(userId);
        project.UpdatedAt = DateTime.UtcNow;

        return await _projectRepository.UpdateAsync(projectId, project);
    }

    public async Task<bool> UpdateProjectStatusAsync(string projectId, ProjectStatus status, string updatedById)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            return false;

        project.Status = status;
        project.UpdatedAt = DateTime.UtcNow;

        return await _projectRepository.UpdateAsync(projectId, project);
    }

    public async Task<bool> IsUserInProjectAsync(string projectId, string userId)
    {
        return await _projectRepository.IsUserInProjectAsync(projectId, userId);
    }
}
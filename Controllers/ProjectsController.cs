using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskManagement.API.Services;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;
using TaskManagement.API.Repositories;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;

    public ProjectsController(
        IProjectService projectService, 
        INotificationService notificationService,
        IUserRepository userRepository)
    {
        _projectService = projectService;
        _notificationService = notificationService;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetProjects([FromQuery] ProjectFilterDto filter)
    {
        try
        {
            var projects = await _projectService.GetAllProjectsAsync();
            var projectDtos = new List<ProjectResponseDto>();
            
            foreach (var project in projects)
            {
                projectDtos.Add(await MapToResponseDto(project));
            }
            
            return Ok(projectDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Projeler alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectResponseDto>> GetProject(string id)
    {
        try
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound(new { message = "Proje bulunamadı" });

            return Ok(await MapToResponseDto(project));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Proje alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("my-projects")]
    public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetMyProjects()
    {
        try
        {
            var userId = GetCurrentUserId();
            var projects = await _projectService.GetProjectsByTeamMemberIdAsync(userId);
            var projectDtos = new List<ProjectResponseDto>();
            
            foreach (var project in projects)
            {
                projectDtos.Add(await MapToResponseDto(project));
            }
            
            return Ok(projectDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Projelerim alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("managed-by-me")]
    public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetManagedProjects()
    {
        try
        {
            var userId = GetCurrentUserId();
            var projects = await _projectService.GetProjectsByManagerIdAsync(userId);
            var projectDtos = new List<ProjectResponseDto>();
            
            foreach (var project in projects)
            {
                projectDtos.Add(await MapToResponseDto(project));
            }
            
            return Ok(projectDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Yönettiğim projeler alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetActiveProjects()
    {
        try
        {
            var projects = await _projectService.GetActiveProjectsAsync();
            var projectDtos = new List<ProjectResponseDto>();
            
            foreach (var project in projects)
            {
                projectDtos.Add(await MapToResponseDto(project));
            }
            
            return Ok(projectDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Aktif projeler alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectResponseDto>> CreateProject(CreateProjectDto createProjectDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var project = await _projectService.CreateProjectAsync(createProjectDto, userId);
            
            // Proje oluşturma bildirimi gönder
            await _notificationService.SendProjectCreatedNotificationAsync(project.Id, userId);

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, await MapToResponseDto(project));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Proje oluşturulurken hata oluştu", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectResponseDto>> UpdateProject(string id, UpdateProjectDto updateProjectDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Kullanıcının bu projeyi güncelleme yetkisi var mı kontrol et
            var existingProject = await _projectService.GetProjectByIdAsync(id);
            if (existingProject == null)
                return NotFound(new { message = "Proje bulunamadı" });
            
            var userRole = GetCurrentUserRole();
            if (userRole != UserRole.Admin && existingProject.ManagerId != userId)
                return Forbid("Bu projeyi güncelleme yetkiniz yok");
            
            var project = await _projectService.UpdateProjectAsync(id, updateProjectDto, userId);
            
            if (project == null)
                return NotFound(new { message = "Proje bulunamadı" });

            return Ok(await MapToResponseDto(project));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Proje güncellenirken hata oluştu", error = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> UpdateProjectStatus(string id, UpdateProjectStatusDto statusDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _projectService.UpdateProjectStatusAsync(id, statusDto.Status, userId);
            
            if (!success)
                return NotFound(new { message = "Proje bulunamadı" });

            return Ok(new { message = "Proje durumu güncellendi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Proje durumu güncellenirken hata oluştu", error = ex.Message });
        }
    }

    [HttpPost("{id}/team-members")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> AddTeamMember(string id, AddTeamMemberDto addMemberDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _projectService.AddTeamMemberAsync(id, addMemberDto.UserId, userId);
            
            if (!success)
                return BadRequest(new { message = "Takım üyesi eklenemedi" });

            return Ok(new { message = "Takım üyesi eklendi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Takım üyesi eklenirken hata oluştu", error = ex.Message });
        }
    }

    [HttpDelete("{id}/team-members/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> RemoveTeamMember(string id, string userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var success = await _projectService.RemoveTeamMemberAsync(id, userId, currentUserId);
            
            if (!success)
                return BadRequest(new { message = "Takım üyesi çıkarılamadı" });

            return Ok(new { message = "Takım üyesi çıkarıldı" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Takım üyesi çıkarılırken hata oluştu", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> DeleteProject(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _projectService.DeleteProjectAsync(id, userId);
            
            if (!success)
                return NotFound(new { message = "Proje bulunamadı" });

            return Ok(new { message = "Proje silindi" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Proje silinirken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("{id}/check-access")]
    public async Task<ActionResult<bool>> CheckUserAccess(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var hasAccess = await _projectService.IsUserInProjectAsync(id, userId);
            return Ok(hasAccess);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erişim kontrolü yapılırken hata oluştu", error = ex.Message });
        }
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
               User.FindFirst("userId")?.Value ?? 
               throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı");
    }

    private UserRole GetCurrentUserRole()
    {
        var roleString = User.FindFirst(ClaimTypes.Role)?.Value ?? 
                        User.FindFirst("role")?.Value;
        
        if (Enum.TryParse<UserRole>(roleString, out var role))
            return role;
        
        return UserRole.Developer;
    }

    private async Task<ProjectResponseDto> MapToResponseDto(Project project)
    {
        // Manager bilgisini al
        var manager = await _userRepository.GetByIdAsync(project.ManagerId);
        
        // Takım üyelerini al
        var teamMembers = new List<ProjectTeamMemberDto>();
        if (project.TeamMemberIds.Any())
        {
            var members = await _userRepository.GetUsersByIdsAsync(project.TeamMemberIds);
            teamMembers = members.Select(m => new ProjectTeamMemberDto
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Email = m.Email,
                Role = m.Role,
                ProfileImageUrl = m.ProfileImageUrl
            }).ToList();
        }

        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            ManagerId = project.ManagerId,
            ManagerName = manager != null ? $"{manager.FirstName} {manager.LastName}" : "Bilinmiyor",
            TeamMembers = teamMembers,
            Status = project.Status,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            Budget = project.Budget,
            Priority = project.Priority,
            Tags = project.Tags,
            Stats = new ProjectStatsDto() // Bu daha sonra task istatistikleri ile doldurulabilir
        };
    }
}
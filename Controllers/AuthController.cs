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
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;

    public AuthController(
        IAuthService authService,
        IJwtTokenService jwtTokenService,
        IUserRepository userRepository)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        try
        {
            var authResponse = await _authService.LoginAsync(loginDto.Email, loginDto.Password);
            
            if (authResponse == null)
                return Unauthorized(new { message = "Geçersiz email veya şifre" });

            return Ok(authResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Giriş yapılırken hata oluştu", error = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
    {
        try
        {
            // Email zaten kullanılıyor mu kontrol et
            var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Bu email adresi zaten kullanılıyor" });

            var authResponse = await _authService.RegisterAsync(registerDto);
            
            return CreatedAtAction(nameof(GetProfile), authResponse);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Kayıt olurken hata oluştu", error = ex.Message });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenDto refreshTokenDto)
    {
        try
        {
            var authResponse = await _authService.RefreshTokenAsync(refreshTokenDto.Token, refreshTokenDto.RefreshToken);
            
            if (authResponse == null)
                return Unauthorized(new { message = "Geçersiz token" });

            return Ok(authResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Token yenilenirken hata oluştu", error = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _authService.LogoutAsync(userId);
            
            return Ok(new { message = "Başarıyla çıkış yapıldı" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Çıkış yapılırken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserResponseDto>> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı" });

            return Ok(MapToUserResponseDto(user));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Profil bilgileri alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<UserResponseDto>> UpdateProfile(UpdateProfileDto updateProfileDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı" });

            // Profil bilgilerini güncelle
            if (!string.IsNullOrEmpty(updateProfileDto.FirstName))
                user.FirstName = updateProfileDto.FirstName;
            
            if (!string.IsNullOrEmpty(updateProfileDto.LastName))
                user.LastName = updateProfileDto.LastName;
            
            if (!string.IsNullOrEmpty(updateProfileDto.PhoneNumber))
                user.PhoneNumber = updateProfileDto.PhoneNumber;
            
            if (!string.IsNullOrEmpty(updateProfileDto.Department))
                user.Department = updateProfileDto.Department;
            
            if (!string.IsNullOrEmpty(updateProfileDto.ProfileImageUrl))
                user.ProfileImageUrl = updateProfileDto.ProfileImageUrl;

            user.UpdatedAt = DateTime.UtcNow;

            var success = await _userRepository.UpdateAsync(userId, user);
            if (!success)
                return BadRequest(new { message = "Profil güncellenemedi" });

            return Ok(MapToUserResponseDto(user));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Profil güncellenirken hata oluştu", error = ex.Message });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _authService.ChangePasswordAsync(userId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            
            if (!success)
                return BadRequest(new { message = "Mevcut şifre yanlış" });

            return Ok(new { message = "Şifre başarıyla değiştirildi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Şifre değiştirilirken hata oluştu", error = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            var success = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
            
            // Güvenlik nedeniyle her zaman başarılı mesajı döndür
            return Ok(new { message = "Eğer email adresiniz sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderildi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Şifre sıfırlama isteği işlenirken hata oluştu", error = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        try
        {
            var success = await _authService.ResetPasswordAsync(resetPasswordDto.Email, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            
            if (!success)
                return BadRequest(new { message = "Geçersiz veya süresi dolmuş token" });

            return Ok(new { message = "Şifre başarıyla sıfırlandı" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Şifre sıfırlanırken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("validate-token")]
    [Authorize]
    public async Task<ActionResult> ValidateToken()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null || !user.IsActive)
                return Unauthorized(new { message = "Geçersiz token" });

            return Ok(new { message = "Token geçerli", user = MapToUserResponseDto(user) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Token doğrulanırken hata oluştu", error = ex.Message });
        }
    }

    // Users endpoints - accessible by Admin, Manager, Developer for task assignment
    [HttpGet("users")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers([FromQuery] UserFilterDto filter)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = users.Select(MapToUserResponseDto).ToList();
            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Kullanıcılar alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("users/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<UserResponseDto>> GetUser(string id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı" });

            return Ok(MapToUserResponseDto(user));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Kullanıcı alınırken hata oluştu", error = ex.Message });
        }
    }

    [HttpPatch("users/{id}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ActivateUser(string id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı" });

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            var success = await _userRepository.UpdateAsync(id, user);
            if (!success)
                return BadRequest(new { message = "Kullanıcı aktifleştirilemedi" });

            return Ok(new { message = "Kullanıcı aktifleştirildi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Kullanıcı aktifleştirilirken hata oluştu", error = ex.Message });
        }
    }

    [HttpPatch("users/{id}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeactivateUser(string id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == id)
                return BadRequest(new { message = "Kendi hesabınızı deaktive edemezsiniz" });

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı" });

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            var success = await _userRepository.UpdateAsync(id, user);
            if (!success)
                return BadRequest(new { message = "Kullanıcı deaktive edilemedi" });

            return Ok(new { message = "Kullanıcı deaktive edildi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Kullanıcı deaktive edilirken hata oluştu", error = ex.Message });
        }
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
               User.FindFirst("userId")?.Value ?? 
               throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı");
    }

    private static UserResponseDto MapToUserResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            IsActive = user.IsActive,
            ProfileImageUrl = user.ProfileImageUrl,
            PhoneNumber = user.PhoneNumber,
            Department = user.Department
        };
    }
}
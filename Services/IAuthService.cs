using TaskManagement.API.DTOs;

namespace TaskManagement.API.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> LoginAsync(string email, string password);
    Task<AuthResponseDto?> RefreshTokenAsync(string token, string refreshToken);
    Task LogoutAsync(string userId);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    Task<UserResponseDto?> GetCurrentUserAsync(string userId);
    Task<bool> ValidateUserCredentialsAsync(string email, string password);
}

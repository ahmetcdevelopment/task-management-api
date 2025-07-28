using TaskManagement.API.Models;
using System.Security.Claims;

namespace TaskManagement.API.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    ClaimsPrincipal? ValidateToken(string token);
    bool ValidateRefreshToken(string refreshToken);
    DateTime GetTokenExpiration(string token);
    string? GetUserIdFromToken(string token);
    UserRole? GetUserRoleFromToken(string token);
}
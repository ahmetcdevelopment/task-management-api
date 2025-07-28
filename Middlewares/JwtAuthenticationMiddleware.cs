using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskManagement.API.Services;

namespace TaskManagement.API.Middlewares;

public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;
    private readonly IJwtTokenService _jwtTokenService;

    public JwtAuthenticationMiddleware(
        RequestDelegate next, 
        ILogger<JwtAuthenticationMiddleware> logger,
        IJwtTokenService jwtTokenService)
    {
        _next = next;
        _logger = logger;
        _jwtTokenService = jwtTokenService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtractTokenFromHeader(context);
        
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var claimsPrincipal = _jwtTokenService.ValidateToken(token);
                if (claimsPrincipal != null)
                {
                    context.User = claimsPrincipal;
                    _logger.LogDebug("JWT token validated successfully for user: {UserId}", 
                        claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                }
                else
                {
                    _logger.LogWarning("Invalid JWT token provided");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate JWT token: {Message}", ex.Message);
            }
        }

        await _next(context);
    }

    private static string? ExtractTokenFromHeader(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(authorizationHeader))
            return null;

        if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authorizationHeader.Substring("Bearer ".Length).Trim();
        }

        return null;
    }
}
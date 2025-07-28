using TaskManagement.API.DTOs;
using TaskManagement.API.Models;
using TaskManagement.API.Repositories;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;

namespace TaskManagement.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IJwtTokenService jwtTokenService, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        // Email zaten var mı kontrol et
        var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return null; // Email zaten kullanımda
        }

        // Şifreyi hash'le
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        // Yeni kullanıcı oluştur
        var user = new User
        {
            Email = registerDto.Email,
            PasswordHash = hashedPassword,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Role = registerDto.Role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            PhoneNumber = registerDto.PhoneNumber,
            Department = registerDto.Department
        };

        var createdUser = await _userRepository.CreateAsync(user);
        var token = _jwtTokenService.GenerateToken(createdUser);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            User = MapToUserResponseDto(createdUser)
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null;
        }

        var token = _jwtTokenService.GenerateToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            User = MapToUserResponseDto(user)
        };
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string token, string refreshToken)
    {
        if (!_jwtTokenService.ValidateRefreshToken(refreshToken))
        {
            return null;
        }

        var userId = _jwtTokenService.GetUserIdFromToken(token);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return null;
        }

        var newToken = _jwtTokenService.GenerateToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        return new AuthResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            User = MapToUserResponseDto(user)
        };
    }

    public async Task LogoutAsync(string userId)
    {
        // Token'ı blacklist'e ekle veya refresh token'ı sil
        // Bu implementasyon için şimdilik boş bırakıyoruz
        await Task.CompletedTask;
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        return await _userRepository.UpdateAsync(userId, user);
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return true; // Güvenlik için her zaman true döndür
        }

        // Burada email gönderme işlemi yapılacak
        // Şimdilik sadece true döndürüyoruz
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        // Token doğrulama işlemi burada yapılacak
        // Şimdilik basit implementasyon
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        return await _userRepository.UpdateAsync(user.Id, user);
    }

    public async Task<UserResponseDto?> GetCurrentUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null ? MapToUserResponseDto(user) : null;
    }

    public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
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

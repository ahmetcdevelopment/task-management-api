using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskManagement.API.DTOs;
using TaskManagement.API.Services;

namespace TaskManagement.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly INotificationService _notificationService;
    private readonly IJwtTokenService _jwtTokenService;

    public NotificationHub(INotificationService notificationService, IJwtTokenService jwtTokenService)
    {
        _notificationService = notificationService;
        _jwtTokenService = jwtTokenService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            // Kullanıcıyı kendi grubuna ekle
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            
            // Kullanıcının rolünü al ve rol grubuna ekle
            var userRole = GetCurrentUserRole();
            if (userRole.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{userRole}");
            }

            // Okunmamış bildirim sayısını gönder
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            await Clients.Caller.SendAsync("UnreadNotificationCount", unreadCount);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            
            var userRole = GetCurrentUserRole();
            if (userRole.HasValue)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"role_{userRole}");
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Kullanıcının bildirimlerini okundu olarak işaretle
    public async Task MarkNotificationAsRead(string notificationId)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return;

        var success = await _notificationService.MarkAsReadAsync(notificationId);
        if (success)
        {
            // Güncellenmiş okunmamış sayıyı gönder
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            await Clients.Caller.SendAsync("UnreadNotificationCount", unreadCount);
            await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
        }
    }

    // Tüm bildirimleri okundu olarak işaretle
    public async Task MarkAllNotificationsAsRead()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return;

        var success = await _notificationService.MarkAllAsReadAsync(userId);
        if (success)
        {
            await Clients.Caller.SendAsync("UnreadNotificationCount", 0);
            await Clients.Caller.SendAsync("AllNotificationsMarkedAsRead");
        }
    }

    // Kullanıcının okunmamış bildirim sayısını al
    public async Task GetUnreadNotificationCount()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return;

        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
        await Clients.Caller.SendAsync("UnreadNotificationCount", unreadCount);
    }

    // Kullanıcıya bildirim gönder (server tarafından çağrılır)
    public async Task SendNotificationToUser(string userId, RealTimeNotificationDto notification)
    {
        await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
        
        // Okunmamış sayıyı güncelle
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
        await Clients.Group($"user_{userId}").SendAsync("UnreadNotificationCount", unreadCount);
    }

    // Belirli bir role sahip tüm kullanıcılara bildirim gönder
    public async Task SendNotificationToRole(string role, RealTimeNotificationDto notification)
    {
        await Clients.Group($"role_{role}").SendAsync("ReceiveNotification", notification);
    }

    // Tüm kullanıcılara bildirim gönder (sistem bildirimleri için)
    public async Task SendNotificationToAll(RealTimeNotificationDto notification)
    {
        await Clients.All.SendAsync("ReceiveNotification", notification);
    }

    // Kullanıcı bir proje grubuna katıl
    public async Task JoinProjectGroup(string projectId)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{projectId}");
    }

    // Kullanıcı bir proje grubundan ayrıl
    public async Task LeaveProjectGroup(string projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project_{projectId}");
    }

    // Proje grubuna bildirim gönder
    public async Task SendNotificationToProject(string projectId, RealTimeNotificationDto notification)
    {
        await Clients.Group($"project_{projectId}").SendAsync("ReceiveNotification", notification);
    }

    // Typing indicator (gelecekte chat özelliği için)
    public async Task StartTyping(string projectId)
    {
        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();
        
        await Clients.GroupExcept($"project_{projectId}", Context.ConnectionId)
            .SendAsync("UserStartedTyping", userId, userName);
    }

    public async Task StopTyping(string projectId)
    {
        var userId = GetCurrentUserId();
        
        await Clients.GroupExcept($"project_{projectId}", Context.ConnectionId)
            .SendAsync("UserStoppedTyping", userId);
    }

    // Yardımcı metodlar
    private string? GetCurrentUserId()
    {
        return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
               Context.User?.FindFirst("userId")?.Value;
    }

    private Models.UserRole? GetCurrentUserRole()
    {
        var roleString = Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? 
                        Context.User?.FindFirst("role")?.Value;
        
        if (Enum.TryParse<Models.UserRole>(roleString, out var role))
            return role;
        
        return null;
    }

    private string? GetCurrentUserName()
    {
        return Context.User?.FindFirst(ClaimTypes.Name)?.Value;
    }
}

// Hub extension methods for easier usage
public static class NotificationHubExtensions
{
    public static async Task SendNotificationToUserAsync(this IHubContext<NotificationHub> hubContext, 
        string userId, RealTimeNotificationDto notification)
    {
        await hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
    }

    public static async Task SendNotificationToRoleAsync(this IHubContext<NotificationHub> hubContext, 
        Models.UserRole role, RealTimeNotificationDto notification)
    {
        await hubContext.Clients.Group($"role_{role}").SendAsync("ReceiveNotification", notification);
    }

    public static async Task SendNotificationToProjectAsync(this IHubContext<NotificationHub> hubContext, 
        string projectId, RealTimeNotificationDto notification)
    {
        await hubContext.Clients.Group($"project_{projectId}").SendAsync("ReceiveNotification", notification);
    }

    public static async Task UpdateUnreadCountAsync(this IHubContext<NotificationHub> hubContext, 
        string userId, long unreadCount)
    {
        await hubContext.Clients.Group($"user_{userId}").SendAsync("UnreadNotificationCount", unreadCount);
    }
}
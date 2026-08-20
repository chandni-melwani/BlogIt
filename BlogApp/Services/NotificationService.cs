using BlogApp.Hubs;
using BlogApp.Models;
using BlogApp.Services;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace BlogApp.Services;

public class NotificationService
{
    private readonly IMongoCollection<Notification> _notifications;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(DatabaseService db, IHubContext<NotificationHub> hubContext)
    {
        _notifications = db.GetCollection<Notification>("notifications");
        _hubContext = hubContext;
    }

    // Event fired whenever a notification is sent.
    // MainLayout subscribes to this to update the bell badge in real-time.
    public event Action<string, string, string>? OnNotificationSent; // (toUserId, message, type)
    public event Action<string>? OnAllRead;
    // Send a notification to a specific user:
    // 1. Save it to MongoDB (persistent)
    // 2. Push it via SignalR (real-time)
    // 3. Fire the event so any listening component updates instantly

    public async Task SendAsync(string toUserId, string fromUserId, string message, string type, string? postId = null)
    {
        // Step 1 — Save to DB
        var notification = new Notification
        {
            ToUserId = toUserId,
            FromUserId = fromUserId,
            Message = message,
            Type = type,
            PostId = postId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notifications.InsertOneAsync(notification);

        // Step 2 — Push to the user's SignalR group instantly
        await _hubContext.Clients
            .Group(toUserId)
            .SendAsync("ReceiveNotification", message, type);

        // Step 3 — Fire the C# event (picked up by MainLayout)
        OnNotificationSent?.Invoke(toUserId, message, type);
    }

    // Get all notifications for a user (newest first)
    public async Task<List<Notification>> GetForUserAsync(string userId) =>
        await _notifications
            .Find(n => n.ToUserId == userId)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();

    // Count unread notifications (for the bell badge)
    public async Task<long> GetUnreadCountAsync(string userId) =>
        await _notifications.CountDocumentsAsync(n =>
            n.ToUserId == userId && !n.IsRead);

    // Mark all notifications as read
    public async Task MarkAllReadAsync(string userId)
    {
        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
        await _notifications.UpdateManyAsync(n => n.ToUserId == userId, update);
        OnAllRead?.Invoke(userId);
    }
}

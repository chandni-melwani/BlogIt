using Microsoft.AspNetCore.SignalR;

namespace BlogApp.Hubs;

public class NotificationHub : Hub
{
    // Called by the client (browser) when they connect.
    // They join a group named after their userId so we can
    // send targeted notifications to just that user.
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }

    // Called by the client to leave their group (e.g. on logout)
    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
    }
}

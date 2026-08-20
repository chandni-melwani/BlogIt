using BlogApp.Models;
using BlogApp.Services;
using MongoDB.Driver;

namespace BlogApp.Data;

public class ConnectionRepository
{
    private readonly IMongoCollection<UserConnection> _connections;

    public ConnectionRepository(DatabaseService db)
    {
        _connections = db.GetCollection<UserConnection>("connections");
    }

    // ── Follow ──────────────────────────────────────────────────────────────

    // Follow someone instantly (no approval needed)
    public async Task FollowAsync(string followerUserId, string followingUserId)
    {
        // Guard: can't follow yourself
        if (followerUserId == followingUserId) return;

        // Guard: don't insert if already following
        var existing = await _connections.Find(c =>
            c.FollowerUserId == followerUserId &&
            c.FollowingUserId == followingUserId &&
            c.Type == "follow").FirstOrDefaultAsync();

        if (existing != null) return;   // already following — do nothing

        var connection = new UserConnection
        {
            FollowerUserId = followerUserId,
            FollowingUserId = followingUserId,
            Type = "follow",
            Status = "accepted"   // follows are always instant
        };
        await _connections.InsertOneAsync(connection);
    }

    // Unfollow someone — deletes the follow record
    public async Task UnfollowAsync(string followerUserId, string followingUserId)
    {
        await _connections.DeleteOneAsync(c =>
            c.FollowerUserId == followerUserId &&
            c.FollowingUserId == followingUserId &&
            c.Type == "follow");
    }

    // ── Friend Requests ─────────────────────────────────────────────────────

    // Send a friend request (starts as "pending")
    public async Task SendFriendRequestAsync(string fromUserId, string toUserId)
    {
        // Guard: can't friend yourself
        if (fromUserId == toUserId) return;

        // Guard: don't insert if a friend connection already exists in either direction
        var existing = await _connections.Find(c =>
            ((c.FollowerUserId == fromUserId && c.FollowingUserId == toUserId) ||
             (c.FollowerUserId == toUserId && c.FollowingUserId == fromUserId)) &&
            c.Type == "friend").FirstOrDefaultAsync();

        if (existing != null) return;

        var connection = new UserConnection
        {
            FollowerUserId = fromUserId,
            FollowingUserId = toUserId,
            Type = "friend",
            Status = "pending"
        };
        await _connections.InsertOneAsync(connection);
    }

    // Accept a pending friend request by its ID
    public async Task AcceptFriendRequestAsync(string connectionId)
    {
        var update = Builders<UserConnection>.Update.Set(c => c.Status, "accepted");
        await _connections.UpdateOneAsync(c => c.Id == connectionId, update);
    }

    // Decline or cancel a friend request — deletes the record
    public async Task DeclineFriendRequestAsync(string connectionId)
    {
        await _connections.DeleteOneAsync(c => c.Id == connectionId);
    }

    // ── Queries ─────────────────────────────────────────────────────────────

    // Get everyone this user follows
    public async Task<List<UserConnection>> GetFollowingAsync(string userId) =>
        await _connections.Find(c =>
            c.FollowerUserId == userId &&
            c.Type == "follow").ToListAsync();

    // Get everyone who follows this user
    public async Task<List<UserConnection>> GetFollowersAsync(string userId) =>
        await _connections.Find(c =>
            c.FollowingUserId == userId &&
            c.Type == "follow").ToListAsync();

    // Get accepted friends for a user (accepted friend connections)
    public async Task<List<UserConnection>> GetFriendsAsync(string userId) =>
        await _connections.Find(c =>
            (c.FollowerUserId == userId || c.FollowingUserId == userId) &&
            c.Type == "friend" &&
            c.Status == "accepted").ToListAsync();

    // Get pending friend requests sent TO this user (so they can accept/decline)
    public async Task<List<UserConnection>> GetPendingRequestsAsync(string userId) =>
        await _connections.Find(c =>
            c.FollowingUserId == userId &&
            c.Type == "friend" &&
            c.Status == "pending").ToListAsync();
    public async Task<UserConnection?> GetConnectionAsync(string fromUserId, string toUserId) =>
    await _connections.Find(c =>
        c.FollowerUserId == fromUserId &&
        c.FollowingUserId == toUserId &&
        c.Type == "friend").FirstOrDefaultAsync();

    // Specifically check if a "follow" relationship exists (not friend)
    public async Task<bool> IsFollowingAsync(string followerUserId, string followingUserId) =>
        await _connections.Find(c =>
            c.FollowerUserId == followerUserId &&
            c.FollowingUserId == followingUserId &&
            c.Type == "follow").AnyAsync();
}

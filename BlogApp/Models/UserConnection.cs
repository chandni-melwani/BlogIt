using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogApp.Models;

public class UserConnection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    // The user who initiated the follow or friend request
    public string FollowerUserId { get; set; } = "";

    // The user being followed or sent a friend request
    public string FollowingUserId { get; set; } = "";

    // "follow" or "friend"
    public string Type { get; set; } = "follow";

    // "accepted" (follows are instant) or "pending" (friend requests wait for approval)
    public string Status { get; set; } = "accepted";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

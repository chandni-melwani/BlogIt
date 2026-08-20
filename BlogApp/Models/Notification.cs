using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogApp.Models;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    // Who RECEIVES this notification (their userId)
    public string ToUserId { get; set; } = "";

    // Who TRIGGERED it (e.g. the person who followed you)
    public string FromUserId { get; set; } = "";

    // Human-readable message e.g. "Someone started following you"
    public string Message { get; set; } = "";

    // "follow" | "friend_request" | "new_post" | "like" | "comment"
    public string Type { get; set; } = "";

    // Which post this notification is about (only set for like/comment types)
    public string? PostId { get; set; }

    // Has the user seen this notification?
    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

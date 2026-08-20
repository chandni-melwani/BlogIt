using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogApp.Models;

public class UserProfile
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string UserId { get; set; } = "";     // Auto-assigned from Supabase — user never types this
    public string Username { get; set; } = "";   // @handle chosen at signup, e.g. "chandni"
    public string Bio { get; set; } = "";        // Optional short description
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


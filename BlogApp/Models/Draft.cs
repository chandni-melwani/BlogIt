using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogApp.Models;

public class Draft
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string UserId { get; set; } = "";
    public string? PostId { get; set; } // null for new post draft, string ID for edit draft

    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Visibility { get; set; } = "Public";
    public List<string> Tags { get; set; } = new();
    public string? ImageUrl { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

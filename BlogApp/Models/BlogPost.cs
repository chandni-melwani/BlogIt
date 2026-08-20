using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogApp.Models;

public class BlogPost
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string AuthorId { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Visibility { get; set; } = "Public";
    public string? ImageUrl { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // AI-generated summary — separate from the manually-typed Summary/subtitle field above.
    // Generated on-demand from PostView, cached here to avoid repeat OpenAI calls.
    public string? AiSummary { get; set; }
    public DateTime? AiSummaryGeneratedAt { get; set; }
}
using BlogApp.Models;
using BlogApp.Services;
using MongoDB.Driver;

namespace BlogApp.Data;

public class EngagementRepository
{
    private readonly IMongoCollection<Like> _likes;
    private readonly IMongoCollection<Comment> _comments;
    private readonly IMongoCollection<SavedPost> _savedPosts;

    public EngagementRepository(DatabaseService db)
    {
        _likes = db.GetCollection<Like>("likes");
        _comments = db.GetCollection<Comment>("comments");
        _savedPosts = db.GetCollection<SavedPost>("savedPosts");
    }

    // ── Likes ──────────────────────────────────────────────────────────

    // Returns true if the post is now liked, false if it was just unliked
    public async Task<bool> ToggleLikeAsync(string postId, string userId)
    {
        var existing = await _likes.Find(l => l.PostId == postId && l.UserId == userId).FirstOrDefaultAsync();

        if (existing is not null)
        {
            await _likes.DeleteOneAsync(l => l.Id == existing.Id);
            return false;
        }

        await _likes.InsertOneAsync(new Like { PostId = postId, UserId = userId });
        return true;
    }

    public async Task<long> GetLikeCountAsync(string postId)
        => await _likes.CountDocumentsAsync(l => l.PostId == postId);

    // Batch version — avoids one DB call per post card on the feed
    public async Task<Dictionary<string, long>> GetLikeCountsAsync(List<string> postIds)
    {
        var results = await _likes.Aggregate()
            .Match(l => postIds.Contains(l.PostId))
            .Group(l => l.PostId, g => new { PostId = g.Key, Count = g.LongCount() })
            .ToListAsync();

        return results.ToDictionary(r => r.PostId, r => r.Count);
    }

    // Which of these posts has the given user liked — for rendering filled vs outline heart icons
    public async Task<HashSet<string>> GetLikedPostIdsAsync(string userId, List<string> postIds)
    {
        var liked = await _likes.Find(l => l.UserId == userId && postIds.Contains(l.PostId)).ToListAsync();
        return liked.Select(l => l.PostId).ToHashSet();
    }

    // ── Comments ───────────────────────────────────────────────────────

    public async Task<Comment> AddCommentAsync(string postId, string userId, string content)
    {
        var comment = new Comment { PostId = postId, UserId = userId, Content = content };
        await _comments.InsertOneAsync(comment);
        return comment;
    }

    public async Task<List<Comment>> GetCommentsAsync(string postId)
        => await _comments.Find(c => c.PostId == postId)
            .SortBy(c => c.CreatedAt)
            .ToListAsync();

    public async Task<long> GetCommentCountAsync(string postId)
        => await _comments.CountDocumentsAsync(c => c.PostId == postId);

    public async Task<Dictionary<string, long>> GetCommentCountsAsync(List<string> postIds)
    {
        var results = await _comments.Aggregate()
            .Match(c => postIds.Contains(c.PostId))
            .Group(c => c.PostId, g => new { PostId = g.Key, Count = g.LongCount() })
            .ToListAsync();

        return results.ToDictionary(r => r.PostId, r => r.Count);
    }

    // ── Saved posts ────────────────────────────────────────────────────

    public async Task<bool> ToggleSaveAsync(string postId, string userId)
    {
        var existing = await _savedPosts.Find(s => s.PostId == postId && s.UserId == userId).FirstOrDefaultAsync();

        if (existing is not null)
        {
            await _savedPosts.DeleteOneAsync(s => s.Id == existing.Id);
            return false;
        }

        await _savedPosts.InsertOneAsync(new SavedPost { PostId = postId, UserId = userId });
        return true;
    }

    public async Task<HashSet<string>> GetSavedPostIdsAsync(string userId, List<string> postIds)
    {
        var saved = await _savedPosts.Find(s => s.UserId == userId && postIds.Contains(s.PostId)).ToListAsync();
        return saved.Select(s => s.PostId).ToHashSet();
    }

    // For the Saved tab on the profile page — all saved post IDs, no filtering by a given list
    public async Task<List<string>> GetAllSavedPostIdsAsync(string userId)
    {
        var saved = await _savedPosts.Find(s => s.UserId == userId)
            .SortByDescending(s => s.CreatedAt)
            .ToListAsync();
        return saved.Select(s => s.PostId).ToList();
    }
}
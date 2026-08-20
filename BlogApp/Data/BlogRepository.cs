using BlogApp.Models;
using BlogApp.Services;
using MongoDB.Driver;

namespace BlogApp.Data;

public class BlogRepository
{
    private readonly IMongoCollection<BlogPost> _posts;
    private readonly IMongoCollection<UserProfile> _profiles;
    private readonly IMongoCollection<Draft> _drafts;

    public BlogRepository(DatabaseService db)
    {
        _posts = db.GetCollection<BlogPost>("posts");
        _profiles = db.GetCollection<UserProfile>("userprofiles");
        _drafts = db.GetCollection<Draft>("drafts");

        try
        {
            var ttlIndex = Builders<Draft>.IndexKeys.Ascending(d => d.UpdatedAt);
            var ttlOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(14) };
            _drafts.Indexes.CreateOne(new CreateIndexModel<Draft>(ttlIndex, ttlOptions));
        }
        catch { }
    }

    public async Task<List<BlogPost>> GetFeedAsync(string currentUserId, List<string> followingUserIds, List<string> friendUserIds)
    {
        var filter = Builders<BlogPost>.Filter.Or(
            Builders<BlogPost>.Filter.Eq(p => p.AuthorId, currentUserId),
            Builders<BlogPost>.Filter.Eq(p => p.Visibility, "Public"),
            Builders<BlogPost>.Filter.And(
                Builders<BlogPost>.Filter.Eq(p => p.Visibility, "Followers"),
                Builders<BlogPost>.Filter.In(p => p.AuthorId, followingUserIds)
            ),
            Builders<BlogPost>.Filter.And(
                Builders<BlogPost>.Filter.Eq(p => p.Visibility, "Private"),
                Builders<BlogPost>.Filter.In(p => p.AuthorId, friendUserIds)
            )
        );

        return await _posts.Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<BlogPost>> GetFollowersSectionAsync(string currentUserId, List<string> followingUserIds)
    {
        var authorIds = followingUserIds.Append(currentUserId).ToList();

        var filter = Builders<BlogPost>.Filter.And(
            Builders<BlogPost>.Filter.Eq(p => p.Visibility, "Followers"),
            Builders<BlogPost>.Filter.In(p => p.AuthorId, authorIds)
        );

        return await _posts.Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<BlogPost>> GetPrivateSectionAsync(string currentUserId, List<string> friendUserIds)
    {
        var authorIds = friendUserIds.Append(currentUserId).ToList();

        var filter = Builders<BlogPost>.Filter.And(
            Builders<BlogPost>.Filter.Eq(p => p.Visibility, "Private"),
            Builders<BlogPost>.Filter.In(p => p.AuthorId, authorIds)
        );

        return await _posts.Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<BlogPost>> GetPublicFeedAsync()
    {
        var filter = Builders<BlogPost>.Filter.Eq(p => p.Visibility, "Public");

        return await _posts.Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<BlogPost>> GetByAuthorIdAsync(string authorId) =>
        await _posts.Find(p => p.AuthorId == authorId).ToListAsync();

    public async Task<BlogPost?> GetByIdAsync(string id)
    {
        try
        {
            return await _posts.Find(p => p.Id == id).FirstOrDefaultAsync();
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public async Task CreateAsync(BlogPost post) =>
        await _posts.InsertOneAsync(post);

    public async Task UpdateAsync(BlogPost post) =>
        await _posts.ReplaceOneAsync(p => p.Id == post.Id, post);

    public async Task DeleteAsync(string id) =>
        await _posts.DeleteOneAsync(p => p.Id == id);

    /// <summary>
    /// Targeted update for just the AI summary fields — uses Update.Set instead of
    /// ReplaceOneAsync so it never clobbers concurrent edits to the rest of the post.
    /// </summary>
    public async Task UpdateAiSummaryAsync(string postId, string summary, DateTime generatedAt)
    {
        var filter = Builders<BlogPost>.Filter.Eq(p => p.Id, postId);
        var update = Builders<BlogPost>.Update
            .Set(p => p.AiSummary, summary)
            .Set(p => p.AiSummaryGeneratedAt, generatedAt);

        await _posts.UpdateOneAsync(filter, update);
    }

    public async Task<List<BlogPost>> GetByIdsAsync(List<string> ids)
    {
        var filter = Builders<BlogPost>.Filter.In(p => p.Id, ids);
        return await _posts.Find(filter).ToListAsync();
    }

    public async Task<List<BlogPost>> SearchPostsAsync(
        string query,
        string? currentUserId,
        List<string>? followingUserIds = null,
        List<string>? friendUserIds = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<BlogPost>();

        var regex = new MongoDB.Bson.BsonRegularExpression(
            System.Text.RegularExpressions.Regex.Escape(query.Trim()), "i");

        var matchingUsers = await _profiles
            .Find(p => p.Username.Contains(query.Trim().ToLower()))
            .ToListAsync();
        var matchingUserIds = matchingUsers.Select(u => u.UserId).ToList();

        var textFilter = Builders<BlogPost>.Filter.Or(
            Builders<BlogPost>.Filter.Regex(p => p.Title,   regex),
            Builders<BlogPost>.Filter.Regex(p => p.Content, regex),
            Builders<BlogPost>.Filter.AnyEq(p => p.Tags, query.Trim()),
            Builders<BlogPost>.Filter.In(p => p.AuthorId, matchingUserIds)
        );

        FilterDefinition<BlogPost> visibilityFilter;

        if (currentUserId is null)
        {
            visibilityFilter = Builders<BlogPost>.Filter.Eq(p => p.Visibility, "Public");
        }
        else
        {
            var following = followingUserIds ?? new List<string>();
            var friends   = friendUserIds   ?? new List<string>();

            visibilityFilter = Builders<BlogPost>.Filter.Or(
                Builders<BlogPost>.Filter.Eq(p => p.AuthorId, currentUserId),
                Builders<BlogPost>.Filter.Eq(p => p.Visibility, "Public"),
                Builders<BlogPost>.Filter.And(
                    Builders<BlogPost>.Filter.Eq(p => p.Visibility, "Followers"),
                    Builders<BlogPost>.Filter.In(p => p.AuthorId, following)
                ),
                Builders<BlogPost>.Filter.And(
                    Builders<BlogPost>.Filter.Eq(p => p.Visibility, "Private"),
                    Builders<BlogPost>.Filter.In(p => p.AuthorId, friends)
                )
            );
        }

        var combinedFilter = Builders<BlogPost>.Filter.And(visibilityFilter, textFilter);

        return await _posts.Find(combinedFilter)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Draft?> GetDraftByIdAsync(string draftId)
    {
        if (string.IsNullOrEmpty(draftId)) return null;
        return await _drafts.Find(d => d.Id == draftId).FirstOrDefaultAsync();
    }

    public async Task<Draft?> GetDraftAsync(string userId, string? postId, string? draftId = null)
    {
        if (!string.IsNullOrEmpty(draftId))
        {
            var byId = await GetDraftByIdAsync(draftId);
            if (byId != null && byId.UserId == userId) return byId;
        }

        if (!string.IsNullOrEmpty(postId))
        {
            return await _drafts.Find(d => d.UserId == userId && d.PostId == postId).FirstOrDefaultAsync();
        }

        return await _drafts.Find(d => d.UserId == userId && d.PostId == null)
            .SortByDescending(d => d.UpdatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task SaveDraftAsync(Draft draft)
    {
        draft.UpdatedAt = DateTime.UtcNow;

        Draft? existing = null;
        if (!string.IsNullOrEmpty(draft.Id))
        {
            existing = await GetDraftByIdAsync(draft.Id);
        }
        else if (!string.IsNullOrEmpty(draft.PostId))
        {
            existing = await _drafts.Find(d => d.UserId == draft.UserId && d.PostId == draft.PostId).FirstOrDefaultAsync();
        }

        if (existing == null)
        {
            await _drafts.InsertOneAsync(draft);
        }
        else
        {
            draft.Id = existing.Id;
            var filter = Builders<Draft>.Filter.Eq(d => d.Id, existing.Id);
            await _drafts.ReplaceOneAsync(filter, draft);
        }
    }

    public async Task DeleteDraftByIdAsync(string draftId)
    {
        if (string.IsNullOrEmpty(draftId)) return;
        await _drafts.DeleteOneAsync(d => d.Id == draftId);
    }

    public async Task DeleteDraftAsync(string userId, string? postId, string? draftId = null)
    {
        if (!string.IsNullOrEmpty(draftId))
        {
            await DeleteDraftByIdAsync(draftId);
            return;
        }

        var filter = Builders<Draft>.Filter.And(
            Builders<Draft>.Filter.Eq(d => d.UserId, userId),
            postId == null
                ? Builders<Draft>.Filter.Eq(d => d.PostId, null)
                : Builders<Draft>.Filter.Eq(d => d.PostId, postId)
        );

        await _drafts.DeleteManyAsync(filter);
    }

    public async Task<List<Draft>> GetDraftsByUserIdAsync(string userId)
    {
        return await _drafts
            .Find(d => d.UserId == userId)
            .SortByDescending(d => d.UpdatedAt)
            .ToListAsync();
    }
}
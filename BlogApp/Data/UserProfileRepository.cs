using BlogApp.Models;
using BlogApp.Services;
using MongoDB.Driver;

namespace BlogApp.Data;

public class UserProfileRepository
{
    private readonly IMongoCollection<UserProfile> _profiles;

    public UserProfileRepository(DatabaseService db)
    {
        _profiles = db.GetCollection<UserProfile>("userprofiles");
    }

    // Get profile by Supabase userId
    public async Task<UserProfile?> GetByUserIdAsync(string userId) =>
        await _profiles.Find(p => p.UserId == userId).FirstOrDefaultAsync();

    // Check if a username is already taken (for validation)
    public async Task<bool> IsUsernameTakenAsync(string username, string currentUserId) =>
        await _profiles.Find(p =>
            p.Username == username.ToLower() &&
            p.UserId != currentUserId).AnyAsync();

    // Save or update a user's profile
    public async Task UpsertAsync(UserProfile profile)
    {
        profile.Username = profile.Username.ToLower().Trim();
        var existing = await GetByUserIdAsync(profile.UserId);

        if (existing == null)
            await _profiles.InsertOneAsync(profile);
        else
            await _profiles.ReplaceOneAsync(p => p.UserId == profile.UserId, profile);
    }

    // Get usernames for a list of userIds in one DB call
    // Returns: Dictionary<userId, username>
    public async Task<Dictionary<string, string>> GetDisplayNamesAsync(List<string> userIds)
    {
        var profiles = await _profiles
            .Find(p => userIds.Contains(p.UserId))
            .ToListAsync();

        // If a user hasn't set a username yet, fall back to first 8 chars of their userId
        var result = new Dictionary<string, string>();
        foreach (var id in userIds)
        {
            var profile = profiles.FirstOrDefault(p => p.UserId == id);
            result[id] = string.IsNullOrEmpty(profile?.Username)
                ? id.Substring(0, 8) + "..."
                : "@" + profile.Username;
        }
        return result;
    }

    // Search user profiles by username or bio (case-insensitive)
    public async Task<List<UserProfile>> SearchUsersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<UserProfile>();

        var trimmed = query.Trim().ToLower();
        var regex = new MongoDB.Bson.BsonRegularExpression(
            System.Text.RegularExpressions.Regex.Escape(trimmed), "i");

        var filter = Builders<UserProfile>.Filter.Or(
            Builders<UserProfile>.Filter.Regex(p => p.Username, regex),
            Builders<UserProfile>.Filter.Regex(p => p.Bio, regex)
        );

        return await _profiles.Find(filter).ToListAsync();
    }
}

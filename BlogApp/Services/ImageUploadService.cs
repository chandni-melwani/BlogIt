using Supabase;
using Microsoft.Extensions.Logging;

namespace BlogApp.Services;

public class ImageUploadService
{
    private readonly Client _supabase;
    private readonly ILogger<ImageUploadService> _logger;
    private const string BucketName = "post-images";
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    public ImageUploadService(Client supabase, ILogger<ImageUploadService> logger)
    {
        _supabase = supabase;
        _logger = logger;
    }

    public async Task<(bool Success, string? Url, string? Error)> UploadCoverImageAsync(
        byte[] fileBytes, string originalFileName, string userId)
    {
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return (false, null, "Only JPG, PNG, WEBP, or GIF images are allowed.");

        if (fileBytes.Length > MaxFileSizeBytes)
            return (false, null, "Image must be smaller than 5MB.");

        var path = $"{userId}/{Guid.NewGuid()}{ext}";

        try
        {
            await _supabase.Storage.From(BucketName).Upload(fileBytes, path);
            var publicUrl = _supabase.Storage.From(BucketName).GetPublicUrl(path);
            return (true, publicUrl, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cover image upload failed for user {UserId}, file {FileName}", userId, originalFileName);
            return (false, null, "We couldn't upload your image. Please try again.");
        }
    }

    /// <summary>
    /// Deletes a previously uploaded cover image from storage, given its public URL.
    /// Safe to call with null/empty/malformed URLs — just no-ops and returns false.
    /// </summary>
    public async Task<bool> DeleteCoverImageAsync(string? publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
            return false;

        var path = ExtractStoragePath(publicUrl);
        if (path is null)
        {
            _logger.LogWarning("Could not extract storage path from URL {Url}; skipping delete.", publicUrl);
            return false;
        }

        try
        {
            await _supabase.Storage.From(BucketName).Remove(new List<string> { path });
            return true;
        }
        catch (Exception ex)
        {
            // Non-fatal: failing to delete an old image should never block saving the new one.
            _logger.LogError(ex, "Failed to delete old cover image at path {Path}", path);
            return false;
        }
    }

    /// <summary>
    /// Pulls the "{userId}/{guid}{ext}" storage path back out of a Supabase public URL.
    /// Public URLs look like: .../storage/v1/object/public/post-images/{userId}/{guid}{ext}
    /// </summary>
    private static string? ExtractStoragePath(string publicUrl)
    {
        var marker = $"/{BucketName}/";
        var idx = publicUrl.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx == -1)
            return null;

        var path = publicUrl[(idx + marker.Length)..];
        return string.IsNullOrWhiteSpace(path) ? null : path;
    }
}
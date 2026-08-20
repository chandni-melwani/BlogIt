using BlogApp.Models;
using System.Text.RegularExpressions;

namespace BlogApp.Helpers;

public static class MarkdownHelper
{
    private const int PreviewLength = 120;
    private const int SmartExcerptLength = 150;

    /// <summary>
    /// Strips common Markdown syntax and returns a plain-text preview,
    /// truncated to <see cref="PreviewLength"/> characters.
    /// Kept as-is for any existing callers (e.g. UserProfile.razor).
    /// </summary>
    public static string StripMarkdown(string? text)
    {
        var stripped = StripSyntax(text);

        return stripped.Length > PreviewLength
            ? stripped.Substring(0, PreviewLength) + "..."
            : stripped;
    }

    /// <summary>
/// Strips Markdown syntax without truncating — used where the full plain-text
/// body is needed (e.g. feeding post content to an AI summarization call).
/// </summary>
public static string StripMarkdownFull(string? text) => StripSyntax(text);

    /// <summary>
    /// Returns the author-written subtitle (BlogPost.Summary) if present;
    /// otherwise falls back to a smart excerpt of the content that ends at the
    /// last full sentence (or at least the last full word) within ~150 characters,
    /// instead of cutting off mid-word/mid-sentence.
    /// </summary>
    public static string GetCardPreview(string? summary, string? content)
    {
        if (!string.IsNullOrWhiteSpace(summary))
            return summary.Trim();

        return SmartExcerpt(content);
    }

    private static string SmartExcerpt(string? text)
    {
        var stripped = StripSyntax(text);
        if (stripped.Length <= SmartExcerptLength) return stripped;

        var window = stripped.Substring(0, SmartExcerptLength);

        // Prefer ending at the last sentence boundary within the window
        var lastSentenceEnd = window.LastIndexOfAny(new[] { '.', '!', '?' });
        if (lastSentenceEnd > SmartExcerptLength / 2) // don't cut too early into the excerpt
            return window.Substring(0, lastSentenceEnd + 1);

        // No good sentence boundary — fall back to the last full word instead
        var lastSpace = window.LastIndexOf(' ');
        return (lastSpace > 0 ? window.Substring(0, lastSpace) : window).TrimEnd() + "...";
    }

    private static string StripSyntax(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";

        // ── Existing basic-markdown rules ──────────────────────────────────
        text = Regex.Replace(text, @"#{1,6}\s*", "");
        text = Regex.Replace(text, @"\*{1,2}([^*]+)\*{1,2}", "$1");
        text = Regex.Replace(text, @"_{1,2}([^_]+)_{1,2}", "$1");
        text = Regex.Replace(text, @"`[^`]+`", "");
        text = Regex.Replace(text, @"\[([^\]]+)\]\([^\)]+\)", "$1");

        // ── GFM extensions added after pipe-table / task-list support ──────

        // Drop entire pipe-table lines (header row, separator row |---|---|, data rows).
        // A table line is any line whose trimmed form starts OR ends with '|'.
        // Removal is intentional: there is no meaningful single-sentence excerpt
        // for tabular data, so the excerpt should skip the table and pick up prose
        // text that follows it.
        text = Regex.Replace(text, @"(?m)^[^\S\r\n]*\|.+\|[^\S\r\n]*$", "");

        // Strip task-list checkboxes but keep the item text.
        // "- [x] Packed Tent" → "Packed Tent"
        // "- [ ] Pack Bag"    → "Pack Bag"
        // Handles both `- [x]` and `* [x]` list markers, case-insensitive.
        text = Regex.Replace(text, @"(?m)^[^\S\r\n]*[-*]\s+\[[xX ]\]\s*", "");

        // Collapse all whitespace runs (including now-empty table lines) into
        // single spaces and trim.
        return Regex.Replace(text, @"\s+", " ").Trim();
    }
}

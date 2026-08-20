using System.Text;
using System.Text.Json;

namespace BlogApp.Services;

public class AiSummaryService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<AiSummaryService> _logger;

    public AiSummaryService(HttpClient http, IConfiguration config, ILogger<AiSummaryService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Generates a summary from plain (already markdown-stripped) post text using Gemini.
    /// Length is allowed to flex — short posts get 2-3 sentences, longer/complex posts
    /// can run to a short paragraph. Never throws — always returns a result tuple.
    /// </summary>
    public async Task<(bool Success, string? Summary, string? Error)> GenerateSummaryAsync(string title, string plainTextContent)
    {
        try
        {
            var apiKey = _config["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Gemini:ApiKey is not configured.");
                return (false, null, "AI summaries aren't configured yet.");
            }

            var model = _config["Gemini:Model"] ?? "gemini-2.5-flash";

            var prompt =
                "You are a concise, engaging blog summarizer. Respond in plain text only, no markdown symbols, " +
                "in exactly this shape:\n\n" +
                "<one overview paragraph, 1-2 sentences, describing what the post is about>\n\n" +
                "Highlights:\n" +
                "- <short highlight or takeaway>\n" +
                "- <short highlight or takeaway>\n" +
                "- <short highlight or takeaway>\n\n" +
                "Use 2-4 highlight bullets, each under 12 words. Only include the 'Highlights:' section if the post " +
                "actually has distinct highlights, tips, or takeaways — if it's a simple post with nothing to list, " +
                "return just the overview paragraph and omit 'Highlights:' entirely. Never pad a simple post.\n\n" +
                $"Title: {title}\n\nContent:\n{Truncate(plainTextContent, 12000)}";

            var requestBody = new
            {
                contents = new object[]
                {
                    new
                    {
                        parts = new object[] { new { text = prompt } }
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = 800,
                    temperature = 0.6,
                    thinkingConfig = new { thinkingLevel = "minimal" }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"models/{model}:generateContent");
            request.Headers.Add("x-goog-api-key", apiKey);
            request.Content = content;

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API error {Status}: {Body}", response.StatusCode, errBody);
                return (false, null, "Couldn't generate a summary right now. Please try again.");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var summary = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(summary))
            {
                return (false, null, "The AI returned an empty summary. Please try again.");
            }

            return (true, summary.Trim(), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AiSummaryService failed to generate a summary.");
            return (false, null, "Something went wrong generating the summary.");
        }
    }

    private static string Truncate(string text, int maxChars) =>
        text.Length <= maxChars ? text : text.Substring(0, maxChars);
}
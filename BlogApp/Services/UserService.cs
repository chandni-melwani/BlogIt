namespace BlogApp.Services;

public class UserService
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public bool IsLoggedIn => UserId != null;

    /// <summary>
    /// Fired by MainLayout after session restoration completes.
    /// Pages subscribe to this to re-render with the correct UserId.
    /// </summary>
    public event Action? OnSessionReady;

    public void NotifySessionReady() => OnSessionReady?.Invoke();
}
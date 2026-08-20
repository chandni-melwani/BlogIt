using MudBlazor;

namespace BlogApp.Helpers;

public static class VisibilityHelper
{
    /// <summary>Returns the CSS class for a visibility badge (matches theme variables).</summary>
    public static string GetVisibilityClass(string visibility) => visibility switch
    {
        "Public"    => "visibility-public",
        "Followers" => "visibility-followers",
        _           => "visibility-private"
    };

    /// <summary>Returns the MudBlazor icon string for a visibility value.</summary>
    public static string GetVisibilityIcon(string visibility) => visibility switch
    {
        "Public"    => Icons.Material.Filled.Public,
        "Followers" => Icons.Material.Filled.People,
        "Private"   => Icons.Material.Outlined.Lock,
        _           => Icons.Material.Filled.Public
    };
}

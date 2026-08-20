using MudBlazor;

namespace BlogApp.Theme;

public static class BlogAppTheme
{
    public static MudTheme Default = new MudTheme
    {
        PaletteLight = new PaletteLight
        {
            Background = "#F7F5F0",
            Surface = "#FFFFFF",
            AppbarBackground = "#F7F5F0",
            AppbarText = "#23281F",
            DrawerBackground = "#F7F5F0",
            DrawerText = "#23281F",
            DrawerIcon = "#666F5C",
            TextPrimary = "#23281F",
            TextSecondary = "#666F5C",
            LinesDefault = "#D8D5C8",
            LinesInputs = "#D8D5C8",
            Divider = "#D8D5C8",
            Primary = "#2F4B3C",
            PrimaryContrastText = "#FFFFFF",
            Success = "#6B8F5A",
            Error = "#A9483B"
        },
        PaletteDark = new PaletteDark
        {
            Background = "#1B1F17",
            Surface = "#242920",
            AppbarBackground = "#1B1F17",
            AppbarText = "#E8E6DC",
            DrawerBackground = "#1B1F17",
            DrawerText = "#E8E6DC",
            DrawerIcon = "#9BA38C",
            TextPrimary = "#E8E6DC",
            TextSecondary = "#A9B09B",
            LinesDefault = "#3A4034",
            LinesInputs = "#3A4034",
            Divider = "#3A4034",
            Primary = "#7FA687",
            PrimaryContrastText = "#1B1F17",
            Success = "#8FBB78",
            Error = "#D97862"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Inter", "Helvetica", "Arial", "sans-serif" },
                FontSize = ".9rem",
                LineHeight = "1.43",
                LetterSpacing = ".01071em"
            },
            Body1 = new Body1Typography
            {
                FontFamily = new[] { "Inter", "Helvetica", "Arial", "sans-serif" },
                FontSize = ".9rem",
                LineHeight = "1.5",
                LetterSpacing = ".00938em"
            },
            Body2 = new Body2Typography
            {
                FontFamily = new[] { "Inter", "Helvetica", "Arial", "sans-serif" },
                FontSize = ".825rem",
                LineHeight = "1.43",
                LetterSpacing = ".01071em"
            },
            Button = new ButtonTypography
            {
                FontFamily = new[] { "Inter", "Helvetica", "Arial", "sans-serif" },
                FontSize = ".85rem",
                LineHeight = "1.75",
                LetterSpacing = ".02857em"
            },
            Caption = new CaptionTypography
            {
                FontFamily = new[] { "Inter", "Helvetica", "Arial", "sans-serif" },
                FontSize = ".75rem",
                LineHeight = "1.66",
                LetterSpacing = ".03333em"
            },
            Subtitle1 = new Subtitle1Typography
            {
                FontFamily = new[] { "Inter", "Helvetica", "Arial", "sans-serif" },
                FontSize = ".95rem",
                LineHeight = "1.5"
            },
            Subtitle2 = new Subtitle2Typography
            {
                FontFamily = new[] { "Inter", "Helvetica", "Arial", "sans-serif" },
                FontSize = ".85rem",
                LineHeight = "1.5"
            },
            H1 = new H1Typography { FontFamily = new[] { "Playfair Display", "serif" }, FontSize = "2.25rem", LineHeight = "1.2" },
            H2 = new H2Typography { FontFamily = new[] { "Playfair Display", "serif" }, FontSize = "1.75rem", LineHeight = "1.3" },
            H3 = new H3Typography { FontFamily = new[] { "Playfair Display", "serif" }, FontSize = "1.5rem", LineHeight = "1.3" },
            H4 = new H4Typography { FontFamily = new[] { "Playfair Display", "serif" }, FontSize = "1.25rem", LineHeight = "1.4" },
            H5 = new H5Typography { FontFamily = new[] { "Playfair Display", "serif" }, FontSize = "1.1rem", LineHeight = "1.4" },
            H6 = new H6Typography { FontFamily = new[] { "Playfair Display", "serif" }, FontSize = "0.95rem", LineHeight = "1.4" }
        }
    };
}
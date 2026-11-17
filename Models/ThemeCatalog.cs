
using DeskWidgets.Themes;

namespace DeskWidgets.Models;

public static class ThemeCatalog
{
    public const string DefaultThemeId = "dark";

    private static readonly IReadOnlyList<AppTheme> Themes =
        BuildCatalog();

    private static readonly Dictionary<string, AppTheme> ThemesById =
        Themes.ToDictionary(
            theme => theme.Id,
            StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<AppTheme> Available => Themes;

    public static AppTheme Get(string? id)
    {
        if (!string.IsNullOrWhiteSpace(id) &&
            ThemesById.TryGetValue(id, out AppTheme? theme))
        {
            return theme;
        }

        return ThemesById[DefaultThemeId];
    }

    private static IReadOnlyList<AppTheme> BuildCatalog()
    {
        List<AppTheme> themes =
        [
            new AppTheme(
                "dark",
                "Dark",
                new ThemePalette(
                    MainBackground: "#101010",
                    CardBackground: "#1D1D1D",
                    ElevatedSurface: "#292929",
                    Accent: "#B9B9B9",
                    PrimaryText: "#F5F5F5",
                    SecondaryText: "#999999",
                    Border: "#343434",
                    Focus: "#FFFFFF")),

            new AppTheme(
                "light",
                "Light",
                new ThemePalette(
                    MainBackground: "#F4F4F2",
                    CardBackground: "#FFFFFF",
                    ElevatedSurface: "#E9E9E6",
                    Accent: "#666660",
                    PrimaryText: "#242424",
                    SecondaryText: "#777772",
                    Border: "#D8D8D4",
                    Focus: "#343434"))
        ];

        themes.AddRange(ExtraThemes.GetAll());

        // Prevent accidental reuse of existing theme IDs.
        HashSet<string> registeredIds =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (AppTheme theme in themes)
        {
            if (string.IsNullOrWhiteSpace(theme.Id))
            {
                throw new InvalidOperationException(
                    "Every theme must have a valid ID.");
            }

            if (!registeredIds.Add(theme.Id))
            {
                throw new InvalidOperationException(
                    $"Duplicate theme ID: '{theme.Id}'.");
            }
        }

        return themes.AsReadOnly();
    }
}

using System.Windows;
using System.Windows.Media;

using DeskWidgets.Models;

namespace DeskWidgets.Services;

public static class ThemeService
{
    public static AppTheme CurrentTheme { get; private set; } =
        ThemeCatalog.Get(ThemeCatalog.DefaultThemeId);

    public static void Apply(string? themeId)
    {
        AppTheme theme = ThemeCatalog.Get(themeId);
        ThemePalette palette = theme.Palette;

        ResourceDictionary resources =
            Application.Current.Resources;

        SetBrush(resources, "MainBackgroundBrush", palette.MainBackground);
        SetBrush(resources, "CardBackgroundBrush", palette.CardBackground);
        SetBrush(resources, "ElevatedSurfaceBrush", palette.ElevatedSurface);
        SetBrush(resources, "AccentBrush", palette.Accent);
        SetBrush(resources, "PrimaryTextBrush", palette.PrimaryText);
        SetBrush(resources, "SecondaryTextBrush", palette.SecondaryText);
        SetBrush(resources, "BorderBrush", palette.Border);
        SetBrush(resources, "FocusBrush", palette.Focus);

        CurrentTheme = theme;
    }

    private static void SetBrush(
        ResourceDictionary resources,
        string key,
        string colorHex)
    {
        Color color = (Color)ColorConverter.ConvertFromString(colorHex);

        resources[key] = new SolidColorBrush(color);
    }
}

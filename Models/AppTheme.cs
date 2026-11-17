namespace DeskWidgets.Models;

public sealed record ThemePalette(
    string MainBackground,
    string CardBackground,
    string ElevatedSurface,
    string Accent,
    string PrimaryText,
    string SecondaryText,
    string Border,
    string Focus);

public sealed record AppTheme(
    string Id,
    string DisplayName,
    ThemePalette Palette);

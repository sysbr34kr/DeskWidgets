using DeskWidgets.Widgets.DigitalClock;

namespace DeskWidgets.Models;

public sealed class ApplicationSettings
{
    public string LanguageId { get; set; } = "en-US";
    public int SchemaVersion { get; set; } = 1;

    public string ThemeId { get; set; } = "dark";

    public int InterfaceScale { get; set; } = 100;

    public WidgetConfigurations Widgets { get; set; } = new();
}

public sealed class WidgetConfigurations
{
    public DigitalClockSettings DigitalClock { get; set; } = new();

    // New modules go here
}

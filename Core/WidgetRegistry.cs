using System;

using DeskWidgets.Models;
using DeskWidgets.Widgets.DigitalClock;

namespace DeskWidgets.Core;

public static class WidgetRegistry
{
    public const string DigitalClockId = "digital-clock";

    public static void RegisterAll(
        WidgetManager manager,
        ApplicationSettings settings,
        Action<string> openSettings)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(openSettings);

        manager.Register(
            DigitalClockId,
            settings.Widgets.DigitalClock,
            widgetSettings =>
            {
                var window = new DigitalClockWindow(widgetSettings);

                window.SettingsRequested += (_, _) =>
                    openSettings(DigitalClockId);

                return window;
            });

        //New modules go here
    }

}

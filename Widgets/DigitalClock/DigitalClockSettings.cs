
namespace DeskWidgets.Widgets.DigitalClock;

public enum DateDisplayFormat
{
    Full,
    Short,
    WeekdayOnly,
    Numeric
}

public sealed class DigitalClockSettings
    : DeskWidgets.Models.WidgetSettings
{
    private double _fontSize = 36;
    private bool _showSeconds = true;
    private bool _use24HourFormat = true;
    private bool _showDate;
    private DateDisplayFormat _dateFormat =
        DateDisplayFormat.Full;

    public double FontSize
    {
        get => _fontSize;
        set => SetField(ref _fontSize, value);
    }

    public bool ShowSeconds
    {
        get => _showSeconds;
        set => SetField(ref _showSeconds, value);
    }

    public bool Use24HourFormat
    {
        get => _use24HourFormat;
        set => SetField(ref _use24HourFormat, value);
    }

    public bool ShowDate
    {
        get => _showDate;
        set => SetField(ref _showDate, value);
    }

    public DateDisplayFormat DateFormat
    {
        get => _dateFormat;
        set => SetField(ref _dateFormat, value);
    }
}

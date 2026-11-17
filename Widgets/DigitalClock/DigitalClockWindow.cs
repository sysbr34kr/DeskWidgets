using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

using DeskWidgets.Core;
using DeskWidgets.Models;

namespace DeskWidgets.Widgets.DigitalClock;

public sealed class DigitalClockWindow : WidgetBase
{
    private readonly DigitalClockSettings _clockSettings;
    private readonly DispatcherTimer _timer;

    private readonly StackPanel _timePanel;
    private readonly FontFamily _clockFont;
    private readonly TextBlock _dateText;

    public DigitalClockWindow(WidgetSettings settings)
        : base(settings)
    {
        _clockSettings = settings as DigitalClockSettings
            ?? throw new ArgumentException(
                "Digital Clock requires DigitalClockSettings.",
                nameof(settings));

        MinWidth = 0;
        MinHeight = 0;

        SizeToContent = SizeToContent.WidthAndHeight;

        var layout = new StackPanel
        {
            Orientation = Orientation.Vertical,
        };

        _clockFont = LoadClockFont();

        _timePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        _dateText = new TextBlock
        {
            FontFamily = new FontFamily("Segoe UI"),
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0)
        };

        _dateText.SetResourceReference(
            TextBlock.ForegroundProperty,
            "SecondaryTextBrush");

        layout.Children.Add(_timePanel);
        layout.Children.Add(_dateText);

        WidgetSurface.Child = layout;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _timer.Tick += Timer_Tick;

        _clockSettings.PropertyChanged +=
            ClockSettings_PropertyChanged;

        Loaded += DigitalClockWindow_Loaded;
        Closed += DigitalClockWindow_Closed;

        ApplyDisplaySettings();
    }

    private void DigitalClockWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        UpdateClock();
        _timer.Start();
    }

    private void Timer_Tick(
        object? sender,
        EventArgs e)
    {
        UpdateClock();
    }

    private void ClockSettings_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(DigitalClockSettings.FontSize):
            case nameof(DigitalClockSettings.ShowSeconds):
            case nameof(DigitalClockSettings.Use24HourFormat):
            case nameof(DigitalClockSettings.ShowDate):
            case nameof(DigitalClockSettings.DateFormat):
                ApplyDisplaySettings();
                break;
        }
    }

    private void ApplyDisplaySettings()
    {
        double fontSize = _clockSettings.FontSize;

        _dateText.FontSize = fontSize * 0.40;

        _dateText.Visibility = _clockSettings.ShowDate
            ? Visibility.Visible
            : Visibility.Collapsed;

        UpdateClock();
    }

    private void UpdateClock()
    {
        DateTime now = DateTime.Now;
        CultureInfo culture = CultureInfo.CurrentCulture;

        string timeFormat;

        if (_clockSettings.Use24HourFormat)
        {
            timeFormat = _clockSettings.ShowSeconds
                ? "HH:mm:ss"
                : "HH:mm";
        }
        else
        {
            timeFormat = _clockSettings.ShowSeconds
                ? "h:mm:ss tt"
                : "h:mm tt";
        }

        string formattedTime = now.ToString(timeFormat, culture);

        if (!_clockSettings.Use24HourFormat)
        {
            int displayedHour = now.Hour % 12;

            if (displayedHour == 0)
                displayedHour = 12;

            if (displayedHour < 10)
                formattedTime = " " + formattedTime;
        }

        RenderTime(formattedTime);

        if (_clockSettings.ShowDate)
        {
            string formattedDate = FormatDate(now, culture);

            _dateText.Text = CapitalizePortugueseDate(
                formattedDate,
                now,
                culture);
        }
    }

    private string FormatDate(
        DateTime date,
        CultureInfo culture)
    {
        return _clockSettings.DateFormat switch
        {
            DateDisplayFormat.Full =>
                FormatFullDate(date, culture),

            DateDisplayFormat.Short =>
                FormatShortTextDate(date, culture),

            DateDisplayFormat.WeekdayOnly =>
                date.ToString("dddd", culture),

            DateDisplayFormat.Numeric =>
                date.ToString("d", culture),

            _ => date.ToString("D", culture)
        };
    }

    private static string FormatFullDate(
        DateTime date,
        CultureInfo culture)
    {
        string fullDate = date.ToString("D", culture);

        string longDatePattern =
            culture.DateTimeFormat.LongDatePattern;

        if (longDatePattern.Contains("dddd"))
        {
            return fullDate;
        }

        string weekday = date.ToString("dddd", culture);

        return $"{weekday}, {fullDate}";
    }

    private static string FormatShortTextDate(
        DateTime date,
        CultureInfo culture)
    {
        string pattern = culture.DateTimeFormat.ShortDatePattern;

        pattern = Regex.Replace(
            pattern,
            "M+",
            "MMM");

        return date.ToString(pattern, culture);
    }

    private void DigitalClockWindow_Closed(
        object? sender,
        EventArgs e)
    {
        _timer.Stop();
        _timer.Tick -= Timer_Tick;

        _clockSettings.PropertyChanged -=
            ClockSettings_PropertyChanged;

        Loaded -= DigitalClockWindow_Loaded;
        Closed -= DigitalClockWindow_Closed;
    }

    private static FontFamily LoadClockFont()
    {
        var baseUri = new Uri("pack://application:,,,/");

        var fontFamily = Fonts.GetFontFamilies(
            baseUri,
            "./Assets/Fonts/")
            .FirstOrDefault();

        return fontFamily ?? throw new InvalidOperationException(
            "The Digital Clock font could not be loaded.");
    }

    private void RenderTime(string time)
    {
        _timePanel.Children.Clear();

        double fontSize = _clockSettings.FontSize;

        double digitWidth = fontSize * 0.85;
        double separatorWidth = fontSize * 0.35;

        string digitsPart = time;
        string? suffixPart = null;

        int suffixSeparatorIndex = time.LastIndexOf(' ');

        if (suffixSeparatorIndex > 0 &&
            suffixSeparatorIndex < time.Length - 1)
        {
            digitsPart = time[..suffixSeparatorIndex];
            suffixPart = time[(suffixSeparatorIndex + 1)..];
        }

        foreach (char character in digitsPart)
        {
            bool isDigit = char.IsDigit(character);

            double width = isDigit || character == ' '
                ? digitWidth
                : separatorWidth;

            var textBlock = new TextBlock
            {
                Text = character.ToString(),
                FontFamily = _clockFont,
                FontSize = fontSize,
                FontWeight = FontWeights.Normal,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = width
            };

            textBlock.SetResourceReference(
                TextBlock.ForegroundProperty,
                "PrimaryTextBrush");

            _timePanel.Children.Add(textBlock);
        }

        if (!string.IsNullOrWhiteSpace(suffixPart))
        {
            var suffixText = new TextBlock
            {
                Text = suffixPart,
                FontFamily = _clockFont,
                FontSize = fontSize * 0.65,
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(fontSize * 0.18, 0, 0, 0)
            };

            suffixText.SetResourceReference(
                TextBlock.ForegroundProperty,
                "PrimaryTextBrush");

            _timePanel.Children.Add(suffixText);
        }
    }

    private static string CapitalizePortugueseDate(
        string text,
        DateTime date,
        CultureInfo culture)
    {
        if (culture.Name != "pt-BR")
            return text;

        string weekday = date.ToString("dddd", culture);
        string month = date.ToString("MMMM", culture);

        static string CapitalizeFirst(string value)
        {
            return string.IsNullOrEmpty(value)
                ? value
                : char.ToUpperInvariant(value[0]) + value[1..];
        }

        text = text.Replace(
            weekday,
            CapitalizeFirst(weekday),
            StringComparison.OrdinalIgnoreCase);

        text = text.Replace(
            month,
            CapitalizeFirst(month),
            StringComparison.OrdinalIgnoreCase);

        return text;
    }

}

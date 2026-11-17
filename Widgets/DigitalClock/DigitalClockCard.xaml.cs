using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using DeskWidgets.Services;

namespace DeskWidgets.Widgets.DigitalClock;

public partial class DigitalClockCard : UserControl
{
    public DigitalClockCard()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty SettingsProperty =
        DependencyProperty.Register(
            nameof(Settings),
            typeof(DigitalClockSettings),
            typeof(DigitalClockCard),
            new PropertyMetadata(null));

    public DigitalClockSettings? Settings
    {
        get => (DigitalClockSettings?)GetValue(SettingsProperty);
        set => SetValue(SettingsProperty, value);
    }

    public static readonly DependencyProperty ResetPositionCommandProperty =
        DependencyProperty.Register(
            nameof(ResetPositionCommand),
            typeof(ICommand),
            typeof(DigitalClockCard),
            new PropertyMetadata(null));

    public ICommand? ResetPositionCommand
    {
        get => (ICommand?)GetValue(ResetPositionCommandProperty);
        set => SetValue(ResetPositionCommandProperty, value);
    }

    
    public sealed class DateFormatOption : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string ResourceKey { get; }

        public DateDisplayFormat Value { get; }

        public string Label =>
            LocalizationService.GetString(ResourceKey);

        public DateFormatOption(
            string resourceKey,
            DateDisplayFormat value)
        {
            ResourceKey = resourceKey;
            Value = value;
        }

        public void RefreshLanguage()
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nameof(Label)));
        }
    }
    
    static DigitalClockCard()
    {
        LocalizationService.LanguageChanged += (_, _) =>
        {
            foreach (var option in DateFormatOptions)
            {
                option.RefreshLanguage();
            }
        };
    }

    public static IReadOnlyList<DateFormatOption> DateFormatOptions { get; } =
    [
        new("Clock_DateFull", DateDisplayFormat.Full),
        new("Clock_DateShort", DateDisplayFormat.Short),
        new("Clock_DateWeekday", DateDisplayFormat.WeekdayOnly),
        new("Clock_DateNumeric", DateDisplayFormat.Numeric)
    ];

    private void BehaviorSettings_ResetPositionRequested(
        object sender,
        RoutedEventArgs e)
    {
        ResetPositionCommand?.Execute(null);
    }

    public static readonly DependencyProperty IsSettingsExpandedProperty =
        DependencyProperty.Register(
            nameof(IsSettingsExpanded),
            typeof(bool),
            typeof(DigitalClockCard),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsSettingsExpanded
    {
        get => (bool)GetValue(IsSettingsExpandedProperty);
        set => SetValue(IsSettingsExpandedProperty, value);
    }

}

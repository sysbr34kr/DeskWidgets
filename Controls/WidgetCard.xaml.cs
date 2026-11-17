
using System.Windows;
using System.Windows.Controls;

namespace DeskWidgets.Controls;

public partial class WidgetCard : UserControl
{
    public WidgetCard()
    {
        InitializeComponent();
    }

    // Widget Title
    public static readonly DependencyProperty WidgetTitleProperty =
        DependencyProperty.Register(
            nameof(WidgetTitle),
            typeof(string),
            typeof(WidgetCard),
            new PropertyMetadata(string.Empty));

    public string WidgetTitle
    {
        get => (string)GetValue(WidgetTitleProperty);
        set => SetValue(WidgetTitleProperty, value);
    }

    // Widget Description
    public static readonly DependencyProperty WidgetDescriptionProperty =
        DependencyProperty.Register(
            nameof(WidgetDescription),
            typeof(string),
            typeof(WidgetCard),
            new PropertyMetadata(string.Empty));

    public string WidgetDescription
    {
        get => (string)GetValue(WidgetDescriptionProperty);
        set => SetValue(WidgetDescriptionProperty, value);
    }

    // Widget Icon
    public static readonly DependencyProperty IconGlyphProperty =
        DependencyProperty.Register(
            nameof(IconGlyph),
            typeof(string),
            typeof(WidgetCard),
            new PropertyMetadata(string.Empty));

    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    // Widget Enabled State
    public static readonly DependencyProperty IsWidgetEnabledProperty =
        DependencyProperty.Register(
            nameof(IsWidgetEnabled),
            typeof(bool),
            typeof(WidgetCard),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsWidgetEnabled
    {
        get => (bool)GetValue(IsWidgetEnabledProperty);
        set => SetValue(IsWidgetEnabledProperty, value);
    }

    // Configuration Expanded State
    public static readonly DependencyProperty IsSettingsExpandedProperty =
        DependencyProperty.Register(
            nameof(IsSettingsExpanded),
            typeof(bool),
            typeof(WidgetCard),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsSettingsExpanded
    {
        get => (bool)GetValue(IsSettingsExpandedProperty);
        set => SetValue(IsSettingsExpandedProperty, value);
    }

    // Widget-Specific Settings Content
    public static readonly DependencyProperty SettingsContentProperty =
        DependencyProperty.Register(
            nameof(SettingsContent),
            typeof(object),
            typeof(WidgetCard),
            new PropertyMetadata(null));

    public object? SettingsContent
    {
        get => GetValue(SettingsContentProperty);
        set => SetValue(SettingsContentProperty, value);
    }
}

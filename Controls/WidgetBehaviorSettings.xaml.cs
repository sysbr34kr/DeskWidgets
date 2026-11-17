using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeskWidgets.Controls;

public partial class WidgetBehaviorSettings : UserControl
{
    public WidgetBehaviorSettings()
    {
        InitializeComponent();
    }

    // Lock Position
    public static readonly DependencyProperty IsPositionLockedProperty =
        DependencyProperty.Register(
            nameof(IsPositionLocked),
            typeof(bool),
            typeof(WidgetBehaviorSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsPositionLocked
    {
        get => (bool)GetValue(IsPositionLockedProperty);
        set => SetValue(IsPositionLockedProperty, value);
    }

    // Transparent Background
    public static readonly DependencyProperty IsBackgroundTransparentProperty =
        DependencyProperty.Register(
            nameof(IsBackgroundTransparent),
            typeof(bool),
            typeof(WidgetBehaviorSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsBackgroundTransparent
    {
        get => (bool)GetValue(IsBackgroundTransparentProperty);
        set => SetValue(IsBackgroundTransparentProperty, value);
    }

    // Always on Top
    public static readonly DependencyProperty IsAlwaysOnTopProperty =
        DependencyProperty.Register(
            nameof(IsAlwaysOnTop),
            typeof(bool),
            typeof(WidgetBehaviorSettings),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsAlwaysOnTop
    {
        get => (bool)GetValue(IsAlwaysOnTopProperty);
        set => SetValue(IsAlwaysOnTopProperty, value);
    }

    // Reset Position Action
    public static readonly DependencyProperty ResetPositionCommandProperty =
        DependencyProperty.Register(
            nameof(ResetPositionCommand),
            typeof(ICommand),
            typeof(WidgetBehaviorSettings),
            new PropertyMetadata(null));

    public ICommand? ResetPositionCommand
    {
        get => (ICommand?)GetValue(ResetPositionCommandProperty);
        set => SetValue(ResetPositionCommandProperty, value);
    }

    public event RoutedEventHandler? ResetPositionRequested;
    private void ResetPositionButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ResetPositionRequested?.Invoke(this, e);
    }

}

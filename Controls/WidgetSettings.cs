using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeskWidgets.Models;

public class WidgetSettings : INotifyPropertyChanged
{
    private bool _isEnabled;
    private bool _isPositionLocked;
    private bool _isBackgroundTransparent;
    private bool _isAlwaysOnTop;

    private double? _left;
    private double? _top;

    public event PropertyChangedEventHandler? PropertyChanged;

    // Whether the widget should be running.
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetField(ref _isEnabled, value);
    }

    // Prevents the user from dragging the widget.
    public bool IsPositionLocked
    {
        get => _isPositionLocked;
        set => SetField(ref _isPositionLocked, value);
    }

    // Controls the widget's background visibility.
    public bool IsBackgroundTransparent
    {
        get => _isBackgroundTransparent;
        set => SetField(ref _isBackgroundTransparent, value);
    }

    // Keeps the widget above ordinary windows.
    public bool IsAlwaysOnTop
    {
        get => _isAlwaysOnTop;
        set => SetField(ref _isAlwaysOnTop, value);
    }

    // Last saved horizontal position.
    public double? Left
    {
        get => _left;
        set => SetField(ref _left, value);
    }

    // Last saved vertical position.
    public double? Top
    {
        get => _top;
        set => SetField(ref _top, value);
    }

    protected bool SetField<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;

        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));

        return true;
    }
}

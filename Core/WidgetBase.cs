using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

using Microsoft.Win32;

using DeskWidgets.Models;
using DeskWidgets.Services;

namespace DeskWidgets.Core;

public abstract class WidgetBase : Window
{
    protected WidgetSettings Settings { get; }
    
    private bool _trackPosition;

    protected Border WidgetSurface { get; }

    public event EventHandler? SettingsRequested;

    protected WidgetBase(WidgetSettings settings)
    {
        Settings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        AllowsTransparency = true;
        ShowInTaskbar = false;

        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.Manual;

        Background = Brushes.Transparent;

        WidgetSurface = new Border
        {
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18, 12, 18, 12)
        };

        Content = WidgetSurface;

        WidgetSurface.ContextMenu = CreateWidgetContextMenu();

        ApplyBackground();
        Topmost = Settings.IsAlwaysOnTop;

        if (Settings.Left is double left &&
            Settings.Top is double top)
        {
            Left = left;
            Top = top;
        }

        Settings.PropertyChanged += OnSettingsChanged;
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        if (Settings.Left is null || Settings.Top is null)
        {
            ResetPosition();
        }
        else
        {
            WidgetPositionService.EnsureVisible(this);
        }

        _trackPosition = true;

        Settings.Left = Left;
        Settings.Top = Top;
    }

    protected override void OnMouseLeftButtonDown(
        MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (e.Handled ||
            Settings.IsPositionLocked ||
            e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        if (IsInteractiveElement(e.OriginalSource))
            return;

        DragMove();
    }

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);

        if (!_trackPosition)
            return;

        if (double.IsNaN(Left) || double.IsNaN(Top))
            return;

        Settings.Left = Left;
        Settings.Top = Top;
    }

    private void OnSettingsChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(WidgetSettings.IsBackgroundTransparent):
                ApplyBackground();
                break;

            case nameof(WidgetSettings.IsAlwaysOnTop):
                Topmost = Settings.IsAlwaysOnTop;
                break;
        }
    }

    private void ApplyBackground()
    {
        if (Settings.IsBackgroundTransparent)
        {
            WidgetSurface.Background = Brushes.Transparent;
        }
        else
        {
            WidgetSurface.SetResourceReference(
                Border.BackgroundProperty,
                "CardBackgroundBrush");
        }
    }

    public void ResetPosition()
    {
        if (!WidgetPositionService.CenterOnPrimaryMonitor(this))
            return;

        Settings.Left = Left;
        Settings.Top = Top;
    }

    private static bool IsInteractiveElement(object source)
    {
        DependencyObject? current = source as DependencyObject;

        while (current is not null)
        {
            if (current is ButtonBase ||
                current is Slider ||
                current is TextBoxBase ||
                current is ComboBox)
            {
                return true;
            }

            current = current is Visual or Visual3D
                ? VisualTreeHelper.GetParent(current)
                : LogicalTreeHelper.GetParent(current);
        }

        return false;
    }

    private void SystemEvents_DisplaySettingsChanged(
        object? sender,
        EventArgs e)
    {
        if (Dispatcher.HasShutdownStarted ||
            Dispatcher.HasShutdownFinished)
        {
            return;
        }

        Dispatcher.BeginInvoke(
            new Action(() =>
            {
                if (!IsLoaded || !IsVisible)
                    return;

                WidgetPositionService.EnsureVisible(this);
            }),
            DispatcherPriority.ContextIdle);
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        SystemEvents.DisplaySettingsChanged +=
            SystemEvents_DisplaySettingsChanged;
    }

    protected override void OnClosed(EventArgs e)
    {
        SystemEvents.DisplaySettingsChanged -=
            SystemEvents_DisplaySettingsChanged;

        Settings.PropertyChanged -= OnSettingsChanged;

        base.OnClosed(e);
    }

    private ContextMenu CreateWidgetContextMenu()
    {
        var menu = new ContextMenu
        {
            Style = (Style)System.Windows.Application.Current.FindResource(
                "WidgetContextMenuStyle")
        };

        var settingsItem = CreateMenuItem(
            "Menu_Settings", 0xE713);

        settingsItem.Click += (_, _) =>
            SettingsRequested?.Invoke(this, EventArgs.Empty);

        menu.Items.Add(settingsItem);
        menu.Items.Add(CreateMenuSeparator());

        menu.Items.Add(CreateToggleMenuItem(
            "Widget_LockPosition",
            0xE72E,
            nameof(WidgetSettings.IsPositionLocked)));

        menu.Items.Add(CreateToggleMenuItem(
            "Widget_TransparentBackground",
            0xE790,
            nameof(WidgetSettings.IsBackgroundTransparent)));

        menu.Items.Add(CreateToggleMenuItem(
            "Widget_AlwaysOnTop",
            0xE718,
            nameof(WidgetSettings.IsAlwaysOnTop)));

        menu.Items.Add(CreateMenuSeparator());

        var closeItem = CreateMenuItem(
            "Menu_Close", 0xE8BB);

        closeItem.Click += (_, _) =>
            Settings.IsEnabled = false;

        menu.Items.Add(closeItem);

        return menu;
    }

    private static Separator CreateMenuSeparator()
    {
        return new Separator
        {
            Style = (Style)System.Windows.Application.Current.FindResource(
                "WidgetMenuSeparatorStyle")
        };
    }

    private MenuItem CreateToggleMenuItem(
        string resourceKey,
        int glyph,
        string propertyName)
    {
        var item = CreateMenuItem(resourceKey, glyph);

        item.IsCheckable = true;

        item.SetBinding(
            MenuItem.IsCheckedProperty,
            new Binding(propertyName)
            {
                Source = Settings,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger =
                    UpdateSourceTrigger.PropertyChanged
            });

        return item;
    }

    private static MenuItem CreateMenuItem(
        string resourceKey,
        int glyph)
    {
        var icon = new TextBlock
        {
            Text = char.ConvertFromUtf32(glyph),
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 16,
            Width = 20,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        icon.SetResourceReference(
            TextBlock.ForegroundProperty,
            "SecondaryTextBrush");

        var item = new MenuItem
        {
            Icon = icon,
            Style = (Style)System.Windows.Application.Current.FindResource(
                "WidgetMenuItemStyle")
        };

        item.SetResourceReference(
            MenuItem.HeaderProperty,
            resourceKey);

        return item;
    }
}

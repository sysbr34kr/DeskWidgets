using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

using DeskWidgets.Models;

namespace DeskWidgets.Core;

public sealed class WidgetManager : IDisposable
{
    private readonly Dictionary<string, WidgetEntry> _widgets = new();

    private bool _disposed;

    public void Register(
        string id,
        WidgetSettings settings,
        Func<WidgetSettings, WidgetBase> windowFactory)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(windowFactory);

        if (_widgets.ContainsKey(id))
        {
            throw new InvalidOperationException(
                $"A widget with ID '{id}' is already registered.");
        }

        var entry = new WidgetEntry(settings, windowFactory);

        _widgets.Add(id, entry);

        PropertyChangedEventHandler handler = (_, e) =>
            OnSettingsChanged(id, e);

        entry.SettingsChangedHandler = handler;
        settings.PropertyChanged += handler;

        if (settings.IsEnabled)
        {
            Start(id);
        }
    }

    public void Start(string id)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        WidgetEntry entry = GetEntry(id);

        if (entry.Window is not null)
            return;

        WidgetBase window = entry.Factory(entry.Settings);

        entry.Window = window;

        window.Closed += (_, _) => OnWindowClosed(id, window);

        window.Show();

        if (!entry.Settings.IsEnabled)
        {
            entry.Settings.IsEnabled = true;
        }
    }

    public void Stop(string id)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        WidgetEntry entry = GetEntry(id);

        WidgetBase? window = entry.Window;

        entry.Window = null;

        window?.Close();

        if (entry.Settings.IsEnabled)
        {
            entry.Settings.IsEnabled = false;
        }
    }


    public void ResetPosition(string id)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        WidgetEntry entry = GetEntry(id);

        if (entry.Window is not null)
        {
            entry.Window.ResetPosition();

            if (entry.Window.WindowState == WindowState.Minimized)
            {
                entry.Window.WindowState = WindowState.Normal;
            }

            entry.Window.Activate();
            entry.Window.Focus();
        }
        else
        {
            entry.Settings.Left = null;
            entry.Settings.Top = null;
        }
    }

    private void OnSettingsChanged(
        string id,
        PropertyChangedEventArgs e)
    {
        if (_disposed)
            return;

        if (e.PropertyName != nameof(WidgetSettings.IsEnabled))
            return;

        WidgetEntry entry = GetEntry(id);

        if (entry.Settings.IsEnabled)
        {
            Start(id);
        }
        else
        {
            Stop(id);
        }
    }

    private void OnWindowClosed(string id, WidgetBase window)
    {
        if (_disposed || !_widgets.TryGetValue(id, out var entry))
            return;

        if (!ReferenceEquals(entry.Window, window))
            return;

        entry.Window = null;

        entry.Settings.IsEnabled = false;
    }

    private WidgetEntry GetEntry(string id)
    {
        if (!_widgets.TryGetValue(id, out var entry))
        {
            throw new KeyNotFoundException(
                $"Widget '{id}' is not registered.");
        }

        return entry;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        foreach (WidgetEntry entry in _widgets.Values)
        {
            if (entry.SettingsChangedHandler is not null)
            {
                entry.Settings.PropertyChanged -=
                    entry.SettingsChangedHandler;
            }

            entry.Window?.Close();
            entry.Window = null;
        }

        _widgets.Clear();
    }

    private sealed class WidgetEntry
    {
        public WidgetSettings Settings { get; }

        public Func<WidgetSettings, WidgetBase> Factory { get; }

        public WidgetBase? Window { get; set; }

        public PropertyChangedEventHandler? SettingsChangedHandler { get; set; }

        public WidgetEntry(
            WidgetSettings settings,
            Func<WidgetSettings, WidgetBase> factory)
        {
            Settings = settings;
            Factory = factory;
        }
    }
}

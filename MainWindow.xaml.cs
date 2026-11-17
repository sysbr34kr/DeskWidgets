using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

using DeskWidgets.Core;
using DeskWidgets.Models;
using DeskWidgets.Services;
using DeskWidgets.Widgets.DigitalClock;

using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfMessageBox = System.Windows.MessageBox;
using WpfApplication = System.Windows.Application;


namespace DeskWidgets;

public partial class MainWindow : Window
{
    private bool _isReady;
    private bool _isExiting;

    private ApplicationTrayService? _trayService;

    private readonly ApplicationSettings _appSettings =
        SettingsService.Load();

    private readonly WidgetManager _widgetManager = new();

    private DigitalClockSettings _digitalClockSettings =>
        _appSettings.Widgets.DigitalClock;

    private bool _widgetsRegistered;
    private bool _updatingStartupToggle;

    public MainWindow()
    {
        InitializeComponent();

        _appSettings.LanguageId =
            LocalizationService.NormalizeLanguage(
                _appSettings.LanguageId);

        LocalizationService.Apply(_appSettings.LanguageId);

        LanguageComboBox.SelectedValue =
            _appSettings.LanguageId;

        _appSettings.ThemeId = NormalizeThemeId(
            _appSettings.ThemeId);

        _appSettings.InterfaceScale = NormalizeInterfaceScale(
            _appSettings.InterfaceScale);

        ThemeService.Apply(_appSettings.ThemeId);

        ThemeComboBox.SelectedValue = _appSettings.ThemeId;

        InterfaceScaleComboBox.SelectedValue =
            _appSettings.InterfaceScale.ToString();

        DigitalClockModule.Settings = _digitalClockSettings;
        
        DigitalClockModule.ResetPositionCommand =
            new RelayCommand(() =>
                _widgetManager.ResetPosition(
                    WidgetRegistry.DigitalClockId));

        _settingsSaveTimer.Tick += SettingsSaveTimer_Tick;

        _digitalClockSettings.PropertyChanged +=
            DigitalClockSettings_PropertyChanged;
        
        StartWithWindowsToggle.IsChecked =
            StartupService.IsEnabled();

        DependencyPropertyDescriptor
            .FromProperty(
                DeskWidgets.Controls.SettingToggle.IsCheckedProperty,
                typeof(DeskWidgets.Controls.SettingToggle))
            .AddValueChanged(
                StartWithWindowsToggle,
                StartWithWindowsToggle_Changed);

        _trayService = new ApplicationTrayService(
            ShowWidgetManager,
            ExitApplication);

    }

    public void InitializeWidgets()
    {
        if (_widgetsRegistered)
            return;

        WidgetRegistry.RegisterAll(
            _widgetManager,
            _appSettings,
            OpenWidgetSettings);

        _widgetsRegistered = true;
    }

    private static string NormalizeThemeId(string? themeId)
    {
        return themeId is "dark" or "light"
            ? themeId
            : "dark";
    }

    private static int NormalizeInterfaceScale(int percentage)
    {
        return percentage is 100 or 125 or 150 or 175 or 200
            ? percentage
            : 100;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        WindowAppearanceService.Apply(
            this,
            ThemeService.CurrentTheme);

        InterfaceScaleService.Apply(
            _appSettings.InterfaceScale,
            this);

        _isReady = true;
    }

    private void InterfaceScaleComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!_isReady)
            return;

        if (sender is not WpfComboBox comboBox ||
            comboBox.SelectedValue is not string selectedValue ||
            !int.TryParse(selectedValue, out int percentage))
        {
            return;
        }

        InterfaceScaleService.Apply(percentage, this);

        _appSettings.InterfaceScale = percentage;
        ScheduleSettingsSave();
    }

    private void ThemeComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!_isReady)
            return;

        if (sender is not WpfComboBox comboBox ||
            comboBox.SelectedValue is not string themeId)
        {
            return;
        }

        ThemeService.Apply(themeId);

        WindowAppearanceService.Apply(
            this,
            ThemeService.CurrentTheme);

        WindowAppearanceService.RefreshTitleBar(this);

        _appSettings.ThemeId = themeId;
        ScheduleSettingsSave();
    }

    protected override void OnClosed(EventArgs e)
    {
        _settingsSaveTimer.Stop();

        _settingsSaveTimer.Tick -= SettingsSaveTimer_Tick;

        _digitalClockSettings.PropertyChanged -=
            DigitalClockSettings_PropertyChanged;

        SaveSettings();

        _widgetManager.Dispose();

        _trayService?.Dispose();
        _trayService = null;

        DependencyPropertyDescriptor
            .FromProperty(
                DeskWidgets.Controls.SettingToggle.IsCheckedProperty,
                typeof(DeskWidgets.Controls.SettingToggle))
            .RemoveValueChanged(
                StartWithWindowsToggle,
                StartWithWindowsToggle_Changed);

        base.OnClosed(e);
    }

    private readonly DispatcherTimer _settingsSaveTimer =
        new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(750)
        };

    private void DigitalClockSettings_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        ScheduleSettingsSave();
    }

    private void ScheduleSettingsSave()
    {
        _settingsSaveTimer.Stop();
        _settingsSaveTimer.Start();
    }

    private void SettingsSaveTimer_Tick(
        object? sender,
        EventArgs e)
    {
        _settingsSaveTimer.Stop();
        SaveSettings();
    }

    private void SaveSettings()
    {
        try
        {
            SettingsService.Save(_appSettings);
        }
        catch (Exception ex) when (
            ex is IOException or
            UnauthorizedAccessException or
            JsonException)
        {
            WpfMessageBox.Show(
                LocalizationService.Format(
                    "Error_SaveSettings",
                    ex.Message),
                "DeskWidgets",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isExiting)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }

    public void ShowWidgetManager()
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(
                new Action(ShowWidgetManager));
            return;
        }

        if (!IsVisible)
            Show();

        if (WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;

        Activate();
        Focus();
    }

    private void ExitApplication()
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(
                new Action(ExitApplication));
            return;
        }

        _isExiting = true;

        Close();

        WpfApplication.Current.Shutdown();
    }

    private void OpenWidgetSettings(string widgetId)
    {
        ShowWidgetManager();

        switch (widgetId)
        {
            case WidgetRegistry.DigitalClockId:
                DigitalClockModule.IsSettingsExpanded = true;
                break;
        }
    }

    private void StartWithWindowsToggle_Changed(
        object? sender,
        EventArgs e)
    {
        if (_updatingStartupToggle)
            return;

        bool enabled = StartWithWindowsToggle.IsChecked;

        try
        {
            StartupService.SetEnabled(enabled);
        }
        catch (Exception ex) when (
            ex is UnauthorizedAccessException or
            IOException or
            System.Security.SecurityException or
            InvalidOperationException)
        {
            _updatingStartupToggle = true;

            try
            {
                StartWithWindowsToggle.IsChecked =
                    StartupService.IsEnabled();
            }
            finally
            {
                _updatingStartupToggle = false;
            }
            
            WpfMessageBox.Show(
                LocalizationService.Format(
                    "Error_StartupSettings",
                    ex.Message),
                "DeskWidgets",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

        }
    }

    private void LanguageComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!_isReady)
            return;

        if (sender is not WpfComboBox comboBox ||
            comboBox.SelectedValue is not string languageId)
        {
            return;
        }

        LocalizationService.Apply(languageId);

        _appSettings.LanguageId = languageId;

        ScheduleSettingsSave();
    }

    private void GitHubLink_RequestNavigate(
        object sender,
        RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = e.Uri.AbsoluteUri,
            UseShellExecute = true
        });

        e.Handled = true;
    }

}

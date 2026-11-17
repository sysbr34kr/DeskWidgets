using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using DeskWidgets.Models;
using DeskWidgets.Widgets.DigitalClock;

namespace DeskWidgets.Services;

public static class SettingsService
{
    private static readonly string SettingsDirectory =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "DeskWidgets");

    private static readonly string SettingsPath =
        Path.Combine(SettingsDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public static string FilePath => SettingsPath;

    public static ApplicationSettings Load()
    {
        if (!File.Exists(SettingsPath))
        {
            return new ApplicationSettings();
        }

        try
        {
            string json = File.ReadAllText(SettingsPath);

            ApplicationSettings settings =
                JsonSerializer.Deserialize<ApplicationSettings>(
                    json,
                    JsonOptions)
                ?? new ApplicationSettings();

            settings.Widgets ??= new WidgetConfigurations();

            settings.Widgets.DigitalClock ??=
                new DigitalClockSettings();

            return settings;
        }
        catch (JsonException)
        {
            return new ApplicationSettings();
        }
        catch (IOException)
        {
            return new ApplicationSettings();
        }
        catch (UnauthorizedAccessException)
        {
            return new ApplicationSettings();
        }
    }

    public static void Save(ApplicationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Directory.CreateDirectory(SettingsDirectory);

        string json = JsonSerializer.Serialize(
            settings,
            JsonOptions);

        string tempPath = SettingsPath + ".tmp";

        try
        {
            File.WriteAllText(tempPath, json);

            File.Move(
                tempPath,
                SettingsPath,
                overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}

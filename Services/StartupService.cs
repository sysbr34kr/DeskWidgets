using System;
using System.IO;
using Microsoft.Win32;

namespace DeskWidgets.Services;

public static class StartupService
{
    private const string RegistryPath =
        @"Software\Microsoft\Windows\CurrentVersion\Run";

    private const string ApplicationName = "DeskWidgets";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            RegistryPath,
            writable: false);

        string? registeredCommand =
            key?.GetValue(ApplicationName) as string;

        string? executablePath = Environment.ProcessPath;

        if (string.IsNullOrWhiteSpace(executablePath))
            return false;

        string expectedCommand =
            $"\"{executablePath}\" --startup";

        return string.Equals(
            registeredCommand,
            expectedCommand,
            StringComparison.OrdinalIgnoreCase);
    }

    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(
            RegistryPath);

        if (key is null)
        {
            throw new InvalidOperationException(
                "Unable to access Windows startup settings.");
        }

        if (!enabled)
        {
            key.DeleteValue(
                ApplicationName,
                throwOnMissingValue: false);

            return;
        }

        string executablePath =
            Environment.ProcessPath
            ?? throw new InvalidOperationException(
                "Unable to locate the application executable.");

        if (!string.Equals(
            Path.GetFileName(executablePath),
            "DeskWidgets.exe",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Startup registration requires DeskWidgets.exe.");
        }

        key.SetValue(
            ApplicationName,
            $"\"{executablePath}\" --startup",
            RegistryValueKind.String);
    }
}

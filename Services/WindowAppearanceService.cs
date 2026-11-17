using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

using DeskWidgets.Models;

namespace DeskWidgets.Services;

public static class WindowAppearanceService
{
    private const int DwmwaDarkMode = 20;
    private const int DwmwaDarkModeLegacy = 19;
    private const int DwmwaCaptionColor = 35;
    private const int DwmwaTextColor = 36;

    private static Color ParseColor(string hex) =>
        (Color)ColorConverter.ConvertFromString(hex);

    private static double GetBrightness(Color color) =>
        (color.R * 0.299) +
        (color.G * 0.587) +
        (color.B * 0.114);

    private static int ToColorRef(Color color) =>
        color.R |
        (color.G << 8) |
        (color.B << 16);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int attribute,
        ref int value,
        int size);

    private const uint WM_NCACTIVATE = 0x0086;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam);

    public static void Apply(Window window, AppTheme theme)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10))
            return;

        IntPtr handle = new WindowInteropHelper(window).Handle;

        if (handle == IntPtr.Zero)
            return;

        ThemePalette palette = theme.Palette;

        Color background = ParseColor(palette.MainBackground);
        Color foreground = ParseColor(palette.PrimaryText);

        bool dark = GetBrightness(background) < 128;
        int darkMode = dark ? 1 : 0;

        int result = DwmSetWindowAttribute(
            handle,
            DwmwaDarkMode,
            ref darkMode,
            sizeof(int));

        if (result != 0)
        {
            DwmSetWindowAttribute(
                handle,
                DwmwaDarkModeLegacy,
                ref darkMode,
                sizeof(int));
        }

        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
            return;

        int captionColor = ToColorRef(background);
        int textColor = ToColorRef(foreground);

        DwmSetWindowAttribute(
            handle,
            DwmwaCaptionColor,
            ref captionColor,
            sizeof(int));

        DwmSetWindowAttribute(
            handle,
            DwmwaTextColor,
            ref textColor,
            sizeof(int));
    }


    public static void RefreshTitleBar(Window window)
    {
        // Windows 10 Workaround
        if (!OperatingSystem.IsWindowsVersionAtLeast(10) ||
            OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            return;
        }

        IntPtr hwnd = new WindowInteropHelper(window).Handle;

        if (hwnd == IntPtr.Zero)
            return;

        SendMessage(
            hwnd,
            WM_NCACTIVATE,
            IntPtr.Zero,
            IntPtr.Zero);

        SendMessage(
            hwnd,
            WM_NCACTIVATE,
            new IntPtr(1),
            IntPtr.Zero);
    }

}

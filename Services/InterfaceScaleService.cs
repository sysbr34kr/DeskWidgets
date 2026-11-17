
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace DeskWidgets.Services;

public static class InterfaceScaleService
{
    public const int DefaultScale = 100;

    public static readonly int[] AvailableScales =
        [100, 125, 150, 175, 200];

    private const double BaseMinimumWidth = 600;
    private const double BaseClientHeight = 410;
    private const double ClientHeightAdjustment = 40;

    // Reserve some space around the window.
    private const double WorkAreaMargin = 32;

    private const uint MonitorDefaultToNearest = 2;

    public static int CurrentScale { get; private set; }
        = DefaultScale;

    public static void Apply(int percentage, Window managerWindow)
    {
        ArgumentNullException.ThrowIfNull(managerWindow);

        if (Array.IndexOf(AvailableScales, percentage) < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(percentage),
                "Unsupported interface scale.");
        }

        double factor = percentage / 100.0;

        // Determine available space on the current monitor.
        Size workArea = GetMonitorWorkArea(managerWindow);

        double availableWidth = Math.Max(
            1,
            workArea.Width - WorkAreaMargin);

        double availableHeight = Math.Max(
            1,
            workArea.Height - WorkAreaMargin);

        // Scale the minimum dimensions, but keep them
        // within the available desktop area.
        double minimumWidth = Math.Min(
            BaseMinimumWidth * factor,
            availableWidth);

        double chromeHeight = GetWindowChromeHeight(managerWindow);

        double minimumHeight = Math.Min(
            BaseClientHeight * factor
            + ClientHeightAdjustment
            + chromeHeight,
            availableHeight);

        bool isDecreasing = percentage < CurrentScale;

        // Preserve current dimensions when increasing scale.
        double currentWidth = double.IsNaN(managerWindow.Width)
            ? managerWindow.ActualWidth
            : managerWindow.Width;

        double currentHeight = double.IsNaN(managerWindow.Height)
            ? managerWindow.ActualHeight
            : managerWindow.Height;

        // Update minimum dimensions.
        managerWindow.MinWidth = minimumWidth;
        managerWindow.MinHeight = minimumHeight;

        if (managerWindow.WindowState == WindowState.Normal)
        {
            if (isDecreasing)
            {
                // Shrink to the new minimum dimensions.
                managerWindow.Width = minimumWidth;
                managerWindow.Height = minimumHeight;
            }
            else
            {
                // Preserve the current size unless adjustment is required.
                managerWindow.Width = Math.Clamp(
                    currentWidth,
                    minimumWidth,
                    availableWidth);

                managerWindow.Height = Math.Clamp(
                    currentHeight,
                    minimumHeight,
                    availableHeight);
            }
        }

        // Workaround for frozen WPF.
        Application.Current.Resources["InterfaceScaleTransform"] =
            new ScaleTransform(factor, factor);

        CurrentScale = percentage;
    }

    private static Size GetMonitorWorkArea(Window window)
    {
        IntPtr handle = new WindowInteropHelper(window).Handle;

        if (handle == IntPtr.Zero)
            return SystemParameters.WorkArea.Size;

        IntPtr monitor = MonitorFromWindow(
            handle,
            MonitorDefaultToNearest);

        var info = new MonitorInfo
        {
            Size = Marshal.SizeOf<MonitorInfo>()
        };

        if (monitor == IntPtr.Zero ||
            !GetMonitorInfo(monitor, ref info))
        {
            return SystemParameters.WorkArea.Size;
        }

        // Win32 gives physical pixel dimensions.
        double pixelWidth =
            info.WorkArea.Right - info.WorkArea.Left;

        double pixelHeight =
            info.WorkArea.Bottom - info.WorkArea.Top;

        // Convert physical pixels into WPF DIPs using
        // the window's current DPI scale.
        uint dpi = GetDpiForWindow(handle);

        double dpiFactor = dpi > 0
            ? dpi / 96.0
            : 1.0;

        return new Size(
            pixelWidth / dpiFactor,
            pixelHeight / dpiFactor);
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(
        IntPtr hwnd,
        uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(
        IntPtr monitor,
        ref MonitorInfo info);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(
        IntPtr hwnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(
        IntPtr hwnd,
        out Rect rect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClientRect(
        IntPtr hwnd,
        out Rect rect);

    [StructLayout(LayoutKind.Sequential)]

    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MonitorInfo
    {
        public int Size;
        public Rect Monitor;
        public Rect WorkArea;
        public uint Flags;
    }

    private static double GetWindowChromeHeight(Window window)
    {
        IntPtr handle = new WindowInteropHelper(window).Handle;

        if (handle == IntPtr.Zero ||
            !GetWindowRect(handle, out Rect windowRect) ||
            !GetClientRect(handle, out Rect clientRect))
        {
            return 0;
        }

        double windowHeight =
            windowRect.Bottom - windowRect.Top;

        double clientHeight =
            clientRect.Bottom - clientRect.Top;

        uint dpi = GetDpiForWindow(handle);

        double dpiFactor = dpi > 0
            ? dpi / 96.0
            : 1.0;

        return Math.Max(
            0,
            (windowHeight - clientHeight) / dpiFactor);
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DeskWidgets.Services;

public static class WidgetPositionService
{
    private const uint MonitorInfoPrimary = 0x00000001;

    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoZOrder = 0x0004;
    private const uint SwpNoActivate = 0x0010;

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int Size;
        public NativeRect Monitor;
        public NativeRect WorkArea;
        public uint Flags;
    }

    private readonly record struct MonitorArea(
        NativeRect WorkArea,
        bool IsPrimary);

    private delegate bool MonitorEnumProc(
        IntPtr monitor,
        IntPtr hdc,
        ref NativeRect rectangle,
        IntPtr data);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayMonitors(
        IntPtr hdc,
        IntPtr clipRect,
        MonitorEnumProc callback,
        IntPtr data);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(
        IntPtr monitor,
        ref MonitorInfo info);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(
        IntPtr hwnd,
        out NativeRect rectangle);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(
        IntPtr hwnd,
        IntPtr insertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);

    private static List<MonitorArea> GetMonitors()
    {
        var monitors = new List<MonitorArea>();

        MonitorEnumProc callback = (
            IntPtr monitor,
            IntPtr hdc,
            ref NativeRect rectangle,
            IntPtr data) =>
        {
            var info = new MonitorInfo
            {
                Size = Marshal.SizeOf<MonitorInfo>()
            };

            if (GetMonitorInfo(monitor, ref info))
            {
                monitors.Add(new MonitorArea(
                    info.WorkArea,
                    (info.Flags & MonitorInfoPrimary) != 0));
            }

            return true;
        };

        EnumDisplayMonitors(
            IntPtr.Zero,
            IntPtr.Zero,
            callback,
            IntPtr.Zero);

        return monitors;
    }

    private static bool TryGetWindowBounds(
        Window window,
        out IntPtr hwnd,
        out NativeRect bounds)
    {
        hwnd = new WindowInteropHelper(window).Handle;

        bounds = default;

        return hwnd != IntPtr.Zero &&
               GetWindowRect(hwnd, out bounds);
    }

    private static long GetIntersectionArea(
        NativeRect window,
        NativeRect workArea)
    {
        int width = Math.Max(
            0,
            Math.Min(window.Right, workArea.Right) -
            Math.Max(window.Left, workArea.Left));

        int height = Math.Max(
            0,
            Math.Min(window.Bottom, workArea.Bottom) -
            Math.Max(window.Top, workArea.Top));

        return (long)width * height;
    }

    private static bool IsFullyVisible(
        NativeRect window,
        NativeRect workArea)
    {
        return window.Left >= workArea.Left &&
               window.Top >= workArea.Top &&
               window.Right <= workArea.Right &&
               window.Bottom <= workArea.Bottom;
    }

    private static bool MoveWindow(
        IntPtr hwnd,
        int left,
        int top)
    {
        return SetWindowPos(
            hwnd,
            IntPtr.Zero,
            left,
            top,
            0,
            0,
            SwpNoSize |
            SwpNoZOrder |
            SwpNoActivate);
    }

    public static bool EnsureVisible(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (!TryGetWindowBounds(
            window,
            out IntPtr hwnd,
            out NativeRect bounds))
        {
            return false;
        }

        List<MonitorArea> monitors = GetMonitors();

        if (monitors.Count == 0)
            return false;

        foreach (MonitorArea monitor in monitors)
        {
            if (IsFullyVisible(bounds, monitor.WorkArea))
                return false;
        }

        MonitorArea target = monitors[0];
        long largestIntersection = -1;

        foreach (MonitorArea monitor in monitors)
        {
            long intersection = GetIntersectionArea(
                bounds,
                monitor.WorkArea);

            if (intersection > largestIntersection ||
                (intersection == largestIntersection &&
                 monitor.IsPrimary))
            {
                largestIntersection = intersection;
                target = monitor;
            }
        }

        NativeRect area = target.WorkArea;

        int left = bounds.Width <= area.Width
            ? Math.Clamp(
                bounds.Left,
                area.Left,
                area.Right - bounds.Width)
            : area.Left;

        int top = bounds.Height <= area.Height
            ? Math.Clamp(
                bounds.Top,
                area.Top,
                area.Bottom - bounds.Height)
            : area.Top;

        if (left == bounds.Left && top == bounds.Top)
            return false;

        return MoveWindow(hwnd, left, top);
    }

    public static bool CenterOnPrimaryMonitor(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (!TryGetWindowBounds(
            window,
            out IntPtr hwnd,
            out NativeRect bounds))
        {
            return false;
        }

        List<MonitorArea> monitors = GetMonitors();

        if (monitors.Count == 0)
            return false;

        MonitorArea target = monitors[0];

        foreach (MonitorArea monitor in monitors)
        {
            if (monitor.IsPrimary)
            {
                target = monitor;
                break;
            }
        }

        NativeRect area = target.WorkArea;

        int left = area.Left +
            Math.Max(0, area.Width - bounds.Width) / 2;

        int top = area.Top +
            Math.Max(0, area.Height - bounds.Height) / 2;

        return MoveWindow(hwnd, left, top);
    }
}

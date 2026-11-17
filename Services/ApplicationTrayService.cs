using System;
using System.Drawing;
using System.Drawing.Text;

using Forms = System.Windows.Forms;

namespace DeskWidgets.Services;

public sealed class ApplicationTrayService : IDisposable
{
    private readonly Forms.NotifyIcon _trayIcon;
    private readonly Forms.ContextMenuStrip _trayMenu;
    private readonly Icon _applicationIcon;
    private readonly Forms.ToolStripMenuItem _openItem;
    private readonly Forms.ToolStripMenuItem _exitItem;

    private bool _disposed;

    public ApplicationTrayService(
        Action openManager,
        Action exitApplication)
    {
        ArgumentNullException.ThrowIfNull(openManager);
        ArgumentNullException.ThrowIfNull(exitApplication);

        _trayMenu = new Forms.ContextMenuStrip();

        _openItem = new Forms.ToolStripMenuItem(
            LocalizationService.GetString("Menu_OpenManager"))
        {
            Image = CreateMenuIcon(
                0xE713,
                Color.DimGray)
        };

        _openItem.Click += (_, _) => openManager();

        _trayMenu.Items.Add(_openItem);

        _trayMenu.Items.Add(
            new Forms.ToolStripSeparator());

        _exitItem = new Forms.ToolStripMenuItem(
            LocalizationService.GetString("Menu_Exit"))
        {
            Image = CreateMenuIcon(
                0xE8BB,
                Color.DimGray)
        };

        _exitItem.Click += (_, _) => exitApplication();

        _trayMenu.Items.Add(_exitItem);

        _applicationIcon =
            Icon.ExtractAssociatedIcon(
                Environment.ProcessPath
                    ?? throw new InvalidOperationException(
                        "Unable to locate the application executable."))
            ?? throw new InvalidOperationException(
                "Unable to load the application icon.");

        _trayIcon = new Forms.NotifyIcon
        {
            Icon = _applicationIcon,
            Text = "DeskWidgets",
            ContextMenuStrip = _trayMenu,
            Visible = true
        };

        _trayIcon.DoubleClick += (_, _) => openManager();

        LocalizationService.LanguageChanged +=
            LocalizationService_LanguageChanged;
    }

    private static Bitmap CreateMenuIcon(
        int glyph,
        Color color)
    {
        const int size = 24;

        var bitmap = new Bitmap(size, size);

        using var graphics = Graphics.FromImage(bitmap);

        graphics.Clear(Color.Transparent);

        graphics.TextRenderingHint =
            TextRenderingHint.AntiAliasGridFit;

        using var font = new Font(
            "Segoe MDL2 Assets",
            14,
            FontStyle.Regular,
            GraphicsUnit.Pixel);

        using var brush = new SolidBrush(color);

        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoWrap
        };

        graphics.DrawString(
            char.ConvertFromUtf32(glyph),
            font,
            brush,
            new RectangleF(0, 0, size, size),
            format);

        return bitmap;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _trayIcon.Visible = false;

        foreach (Forms.ToolStripItem item in _trayMenu.Items)
        {
            item.Image?.Dispose();
            item.Image = null;
        }

        LocalizationService.LanguageChanged -=
            LocalizationService_LanguageChanged;

        _trayIcon.Dispose();
        _trayMenu.Dispose();
        _applicationIcon.Dispose();
    }

    private void LocalizationService_LanguageChanged(
        object? sender,
        EventArgs e)
    {
        if (_disposed)
            return;

        _openItem.Text =
            LocalizationService.GetString("Menu_OpenManager");

        _exitItem.Text =
            LocalizationService.GetString("Menu_Exit");
    }

}

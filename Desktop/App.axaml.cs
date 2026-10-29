using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.BeaconSDK.Helpers.IO;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using Tavstal.KonkordLauncher.Desktop.Views;

namespace Tavstal.KonkordLauncher.Desktop;

// ReSharper disable once PartialTypeWithSinglePart - Avalonia code generation
public partial class App : Application
{
    private static IBeaconLogger? _logger;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    ///     Configures the application after the framework initialization is completed.
    ///     Sets up the main window for the desktop-style application lifetime.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (Design.IsDesignMode)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            throw new InvalidOperationException("The application had invalid lifetime.");

        var services = Program.Services;
        _logger = services.GetRequiredService<IBeaconLogger<App>>();
        var eventService = services.GetRequiredService<IEventService>();
        eventService.OnThemeChanged += ApplyTheme;

        desktop.MainWindow = services.GetRequiredService<StartupWindow>();
        desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;


#if DEBUG
        this.AttachDeveloperTools();
#endif

        var launcherStore = services.GetRequiredService<ILauncherStore>();
        desktop.ShutdownRequested += (_, _) =>
        {
            // Try deleting the temp directory on shutdown, but ignore any exceptions that may occur.
            try
            {
                FileSystemHelper.DeleteDirectory(launcherStore.GetTempDir());
            }
            catch (Exception)
            {
                // ignored
            }
        };

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    ///     Applies the specified theme to the application by setting the requested theme variant.
    /// </summary>
    /// <param name="theme">The theme to apply, either Light or Dark.</param>
    private void ApplyTheme(EThemeType theme)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            try
            {
                if (Current == null)
                    return;
                RequestedThemeVariant = theme switch
                {
                    EThemeType.LIGHT => ThemeVariant.Light,
                    EThemeType.DARK => ThemeVariant.Dark,
                    _ => RequestedThemeVariant
                };
            }
            catch (Exception ex)
            {
                _logger?.LogCritical(ex, "Failed to apply theme");
            }
        });
    }
}

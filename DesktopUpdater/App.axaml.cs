using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.DesktopUpdater.Views;

namespace Tavstal.KonkordLauncher.DesktopUpdater;

// ReSharper disable once PartialTypeWithSinglePart - Avalonia code generation
public partial class App : Application
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(App));
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        try
        {
            var settings = LauncherHelper.GetLauncherSettings();
            ApplyTheme(settings.Launcher.Theme);
        }
        catch
        {
            // ignored
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void ApplyTheme(EThemeType theme)
    {
        try
        {
            if (Current == null)
                return;
            switch (theme)
            {
                case EThemeType.Light:
                {
                    RequestedThemeVariant = ThemeVariant.Light;
                    break;
                }
                case EThemeType.Dark:
                {
                    RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to apply theme");
            _logger.Error(ex);
        }
    }
}
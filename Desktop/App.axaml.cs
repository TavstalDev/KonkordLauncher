using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Tavstal.KonkordLauncher.Common.Models;

namespace Tavstal.KonkordLauncher.Desktop;

/// <summary>
/// Represents the main application class for the Konkord Launcher desktop application.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the application by loading XAML resources.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Configures the application after the framework initialization is completed.
    /// Sets up the main window for the desktop-style application lifetime.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Sets the main window to the StartupWindow, passing the application lifetime.
            desktop.MainWindow = new Views.StartupWindow(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    /// <summary>
    /// Delegate for handling theme change events.
    /// </summary>
    /// <param name="newTheme">The new theme that has been applied.</param>
    public delegate void ThemeChangedEventHandler(EThemeType newTheme);

    /// <summary>
    /// Event triggered when the application theme is changed.
    /// </summary>
    public static event ThemeChangedEventHandler? OnThemeChanged;

    /// <summary>
    /// Invokes the theme changed event with the specified new theme.
    /// </summary>
    /// <param name="newTheme">The new theme to apply.</param>
    public static void InvokeThemeChanged(EThemeType newTheme)
    {
        OnThemeChanged?.Invoke(newTheme);
    }

    /// <summary>
    /// Delegate for handling language change events.
    /// </summary>
    /// <param name="newLanguage">The new language that has been applied.</param>
    public delegate void LanguageChangedEventHandler(string newLanguage);

    /// <summary>
    /// Event triggered when the application language is changed.
    /// </summary>
    public static event LanguageChangedEventHandler? OnLanguageChanged;

    /// <summary>
    /// Invokes the language changed event with the specified new language.
    /// </summary>
    /// <param name="newLanguage">The new language to apply.</param>
    public static void InvokeLanguageChanged(string newLanguage)
    {
        OnLanguageChanged?.Invoke(newLanguage);
    }
}
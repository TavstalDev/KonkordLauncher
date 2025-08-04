using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Desktop;

/// <summary>
/// Represents the main application class for the Konkord Launcher desktop application.
/// </summary>
public partial class App : Application
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(App));
    private static PixelSize _screenSize = new(1920, 1080);
    public static PixelSize ScreenSize => _screenSize;
    
    public static decimal ScreenWidth => _screenSize.Width;
    public static decimal ScreenHeight => _screenSize.Height;
    public static void SetScreenSize(PixelSize screenSize)
    {
        _screenSize = screenSize;
    }

    private static string _version;
    public static string Version
    {
        get
        {
            if (!string.IsNullOrEmpty(_version))
                return _version;
            
            Version? currentVersion;
            object[] versionAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            if (versionAttributes.Length > 0)
            {
                AssemblyInformationalVersionAttribute informationalVersionAttribute = (AssemblyInformationalVersionAttribute)versionAttributes[0];
                currentVersion = new Version(informationalVersionAttribute.InformationalVersion);
            }
            else
                currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            
            _version = currentVersion?.ToString() ?? "2.0.0";
            return _version;
        }
    }
    public static string Branch => "stable";
    public static string BuildDate
    {
        get // TODO: Use a more reliable method to get the build date
        {
            object[] buildDateAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyMetadataAttribute), false);
            foreach (var attribute in buildDateAttributes)
            {
                if (attribute is AssemblyMetadataAttribute metadata && metadata.Key == "BuildDate")
                {
                    return metadata.Value ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
                }
            }
            return DateTime.UtcNow.ToString("yyyy-MM-dd");
        }
    }
    
    /// <summary>
    /// Initializes the application by loading XAML resources.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        OnThemeChanged += ApplyTheme;

        try
        {
            var settings = LauncherHelper.GetLauncherSettings();
            ApplyTheme(settings.Launcher.Theme);
            //InvokeLanguageChanged(settings.Launcher.Language);
        }
        catch (Exception _)
        {
            // ignored
        }
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

    #region Events
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
    /// Applies the specified theme to the application by setting the requested theme variant.
    /// </summary>
    /// <param name="theme">The theme to apply, either Light or Dark.</param>
    public void ApplyTheme(EThemeType theme)
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
        Task.Run(async () =>
        {
            var settings = await LauncherHelper.GetLauncherSettingsAsync();
            if (!await TranslationManager.EnsureLanguageFileExistsAsync(newLanguage))
                return;
            
            var result = await TranslationManager.ReadTranslationAsync(Path.Combine(settings.Launcher.TranslationsDirectoryPath, $"{newLanguage}.json"));
            TranslationManager.SetTranslations(newLanguage, result);
        });
        OnLanguageChanged?.Invoke(newLanguage);
    }

    /// <summary>
    /// Delegate for handling accounts changed events.
    /// </summary>
    public delegate void AccountsChangedEventHandler();

    /// <summary>
    /// Event triggered when the accounts data is changed.
    /// </summary>
    public static event AccountsChangedEventHandler? OnAccountsChanged;

    /// <summary>
    /// Invokes the accounts changed event to notify subscribers of changes in the accounts data.
    /// </summary>
    public static void InvokeAccountsChanged()
    {
        OnAccountsChanged?.Invoke();
    }
    #endregion
}
using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveUI.Avalonia;
using Tavstal.BeaconSDK;
using Tavstal.BeaconSDK.Helpers.IO;
using Tavstal.BeaconSDK.Models.Config;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Accounts;
using Tavstal.KonkordLauncher.Desktop.Services.Content;
using Tavstal.KonkordLauncher.Desktop.Services.Instances;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation;
using Tavstal.KonkordLauncher.Desktop.Services.Startup;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using Tavstal.KonkordLauncher.Desktop.Services.Update;
using Tavstal.KonkordLauncher.Desktop.Views;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;
using Tavstal.KonkordLauncher.Desktop.Views.Models;
using Velopack;

namespace Tavstal.KonkordLauncher.Desktop;


class Program
{
    public static IServiceProvider Services { get; private set; } = null!;

    #region Versioning

    public static string Version
    {
        get
        {
            if (!string.IsNullOrEmpty(field))
                return field;

            Version? currentVersion = null;
            try
            {
                object[] versionAttributes = Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
                if (versionAttributes.Length > 0)
                {
                    AssemblyInformationalVersionAttribute informationalVersionAttribute =
                        (AssemblyInformationalVersionAttribute)versionAttributes[0];
                    currentVersion = new Version(informationalVersionAttribute.InformationalVersion);
                }
                else
                    currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            }
            catch { /* ignored */}

            field = currentVersion?.ToString() ?? "2.0.0";
            return field;
        }
    } = string.Empty;

    public static string Branch
    {
        get
        {
#if  DEBUG
            return "dev";
#else
            return "stable";
#endif
        }
    }
    private static string _buidDate = string.Empty;
    public static string BuildDate
    {
        get
        {
            if (!string.IsNullOrEmpty(_buidDate))
                return _buidDate;

            object[] buildDateAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyMetadataAttribute), false);
            foreach (var attribute in buildDateAttributes)
            {
                if (attribute is AssemblyMetadataAttribute { Key: "BuildDate" } metadata)
                {
                    _buidDate = metadata.Value ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
                    return _buidDate;
                }
            }

            _buidDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            return _buidDate;
        }
    }
    #endregion

    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        string appDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "KonkordLauncher"
        );
        Directory.CreateDirectory(appDir);
        string keysDir = Path.Combine(appDir, "keys");
        Directory.CreateDirectory(keysDir);
        string logsDir = Path.Combine(appDir, "logs");
        Directory.CreateDirectory(logsDir);
        string latestLog =  Path.Combine(logsDir, "latest.log");
        if (File.Exists(latestLog))
        {
            var lastModified = File.GetLastWriteTime(latestLog);
            string archivePath = Path.Combine(logsDir, $"{lastModified:yyyy-MM-dd_HH-mm-ss}.log.gz");
            FileSystemHelper.CompressFile(latestLog, archivePath);
        }

        var beaconConfig = new BeaconConfiguration
        {
            Auth = new AuthConfiguration
            {
                MicrosoftClientId = "496a0c42-aa74-41fe-b7bc-0ad155cdaa26"
            },
            DataProtection = new DataProtectionConfig
            {
                ApplicationName = "KonkordLauncher",
                ProtectorPurpose = "KonkordLauncher.Encryption.v1",
                KeysDirectory = keysDir,
                WindowsEntropy = [.. "9fJVajUUeynaqk3pvuoRkxjD5hsXRLMQ"u8],
                LinuxKey = [.. "oN93VTwahenTgCCMoRJSswt3nkkZ7KFY"u8],
                MacKey = [.. "7yx7QoAwLg2zPtwVsRYoXNA4Vtef2tVX"u8]
            },
            Http = new HttpConfig
            {
                Agent = "KonkordLauncher/2.0.0 (+https://github.com/TavstalDev/KonkordLauncher)",
            },
            Logging = new LoggingConfig
            {
                EnableCustomLogger = false,
                DefaultLogFilePath = latestLog,
#if DEBUG
                LogLevel = LogLevel.Debug
#else
                LogLevel = LogLevel.Information
#endif
            }
        };

        var appHost = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddBeaconServices(beaconConfig);
                services.AddSingleton<IEventService,  EventService>();
                services.AddSingleton<IUpdateService, UpdateService>();
                services.AddSingleton<IDiscordPresenceService, DiscordPresenceService>();

                services.AddSingleton<ISkinService, SkinService>();
                services.AddSingleton<ILauncherStore, LauncherStore>();
                services.AddSingleton<IScreenService, ScreenService>();
                services.AddSingleton<IWindowFactory, WindowFactory>();
                services.AddSingleton<IInstanceModelFactory, InstanceModelFactory>();

                services.AddSingleton<ITranslationService, TranslationService>();
                services.AddHostedService(sp => sp.GetRequiredService<TranslationService>());

                services.AddSingleton<IValidationService, ValidationService>();
                services.AddSingleton<IModrinthApiClient, ModrinthApiClient>();
                services.AddSingleton<ModrinthPackageService, ModrinthPackageService>();
                services.AddSingleton<CurseForgePackageService, CurseForgePackageService>();
                services.AddSingleton<IBitmapService, BitmapService>();
                services.AddSingleton<IMetaCacheService, MetaCacheService>();
                services.AddHostedService(sp => sp.GetRequiredService<MetaCacheService>());

                services.AddTransient<StartupViewModel>();
                services.AddTransient<StartupWindow>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainWindow>();
                services.AddTransient<CreateInstanceViewModel>();
                services.AddTransient<CreateInstanceWindow>();
                services.AddTransient<AccountsViewModel>();
                services.AddTransient<AccountsWindow>();
                services.AddTransient<JavaSelectorViewModel>();
                services.AddTransient<JavaSelectorWindow>();
                services.AddTransient<IconSelectorViewModel>();
                services.AddTransient<IconSelectorWindow>();
            })
            .Build();

        Services = appHost.Services;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Configures and returns the Avalonia <see cref="AppBuilder"/> used to start the UI.
    /// This method is intentionally static and called by the runtime; it is also used by the visual designer.
    /// </summary>
    /// <returns>A configured <see cref="AppBuilder"/> instance for Avalonia.</returns>
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI(_ => { });
    }
}

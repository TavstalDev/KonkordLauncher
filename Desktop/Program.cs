using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
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
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Startup;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using Tavstal.KonkordLauncher.Desktop.Services.Update;
using Tavstal.KonkordLauncher.Desktop.Views;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;
using Tavstal.KonkordLauncher.Desktop.Views.Models;
using Velopack;

namespace Tavstal.KonkordLauncher.Desktop;

internal class Program
{
    private const string MutexName = "Global\\KonkordLauncher_5C0F1C0A-2258-4DD5-B454-0ED8FFF9E5A8";
    private static Mutex? Mutex { get; set; }
    public static IServiceProvider Services { get; private set; } = null!;

    [STAThread]
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [RequiresDynamicCode("Calls Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder()")]
    public static void Main(string[] args)
    {
        // Prevent multiple instances of the application from running simultaneously
        Mutex = new Mutex(initiallyOwned: true, MutexName, out bool isNewInstance);
        if (!isNewInstance)
            return;

        VelopackApp.Build().Run();

        var appDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "KonkordLauncher"
        );
        Directory.CreateDirectory(appDir);
        var keysDir = Path.Combine(appDir, "keys");
        Directory.CreateDirectory(keysDir);

        string workDir = Directory.GetCurrentDirectory();
        var logsDir = Path.Combine(workDir, "logs");
        Directory.CreateDirectory(logsDir);
        var latestLog = Path.Combine(logsDir, "latest.log");

        var fileInfo = new FileInfo(latestLog);
        if (fileInfo.Exists)
        {
            var newPath = Path.Combine(logsDir, $"{fileInfo.LastWriteTime:yyyy-MM-dd_HH-mm-ss}.log");
            var archivePath = newPath + ".gz";
            File.Move(latestLog, newPath, true);
            FileSystemHelper.CompressFile(newPath, archivePath);
        }

        var beaconConfig = new BeaconConfiguration
        {
            Auth = new AuthConfiguration { MicrosoftClientId = "496a0c42-aa74-41fe-b7bc-0ad155cdaa26" },
            DataProtection = new DataProtectionConfig
            {
                ApplicationName = "KonkordLauncher",
                ProtectorPurpose = "KonkordLauncher.Encryption.v1",
                KeysDirectory = keysDir,
                WindowsEntropy = [.. "9fJVajUUeynaqk3pvuoRkxjD5hsXRLMQ"u8],
                LinuxKey = [.. "oN93VTwahenTgCCMoRJSswt3nkkZ7KFY"u8],
                MacKey = [.. "7yx7QoAwLg2zPtwVsRYoXNA4Vtef2tVX"u8]
            },
            Http = new HttpConfig { Agent = "KonkordLauncher/2.0.0 (+https://github.com/TavstalDev/KonkordLauncher)" },
            Logging = new LoggingConfig
            {
                EnableCustomLogger = true,
                DefaultLogFilePath = latestLog,
#if DEBUG
                LogLevel = LogLevel.Debug
#else
                LogLevel = LogLevel.Information
#endif
            }
        };

        using var appHost = Host.CreateDefaultBuilder(args)
            .ConfigureHostOptions(opt =>
            {
                opt.ShutdownTimeout = TimeSpan.FromSeconds(3);
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<IHostLifetime, NoopHostLifetime>();
                services.AddBeaconServices(beaconConfig);
                services.AddSingleton<IEventService, EventService>();
                services.AddSingleton<IUpdateService, UpdateService>();
                services.AddSingleton<IDiscordPresenceService, DiscordPresenceService>();
                services.AddSingleton<ITopLevelProvider, TopLevelProvider>();
                services.AddSingleton<IWindowFactory, WindowFactory>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<IFilePickerService, FilePickerService>();

                services.AddSingleton<ISkinService, SkinService>();
                services.AddSingleton<ILauncherStore, LauncherStore>();
                services.AddSingleton<IScreenService, ScreenService>();
                services.AddSingleton<IInstanceModelFactory, InstanceModelFactory>();

                services.AddSingleton<ITranslationService, TranslationService>();
                services.AddHostedService(sp => (TranslationService)sp.GetRequiredService<ITranslationService>());

                services.AddSingleton<IValidationService, ValidationService>();
                services.AddSingleton<IModrinthApiClient, ModrinthApiClient>();
                services.AddSingleton<ModrinthPackageService, ModrinthPackageService>();
                services.AddSingleton<CurseForgePackageService, CurseForgePackageService>();
                services.AddSingleton<IBitmapService, BitmapService>();
                services.AddHostedService(sp => (BitmapService)sp.GetRequiredService<IBitmapService>());
                services.AddSingleton<IMetaCacheService, MetaCacheService>();
                services.AddHostedService(sp => (MetaCacheService)sp.GetRequiredService<IMetaCacheService>());

                // Startup window
                services.AddTransient<StartupViewModel>();
                services.AddTransient<StartupWindow>();
                // Main window
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainWindow>();
                // Create Instance Window
                services.AddTransient<CreateInstanceViewModel>();
                services.AddTransient<CreateInstanceWindow>();
                // Accounts Window
                services.AddTransient<AccountsViewModel>();
                services.AddTransient<AccountsWindow>();
                // Java Selector Window
                services.AddTransient<JavaSelectorViewModel>();
                services.AddTransient<JavaSelectorWindow>();
                // Icon Selector Window
                services.AddTransient<IconSelectorViewModel>();
                services.AddTransient<IconSelectorWindow>();
            })
            .Build();
        Services = appHost.Services;

        try
        {
            appHost.Start();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            appHost.StopAsync().GetAwaiter().GetResult();
            Mutex.ReleaseMutex();
            Mutex.Dispose();
        }
    }

    /// <summary>
    ///     Configures and returns the Avalonia <see cref="AppBuilder" /> used to start the UI.
    ///     This method is intentionally static and called by the runtime; it is also used by the visual designer.
    /// </summary>
    /// <returns>A configured <see cref="AppBuilder" /> instance for Avalonia.</returns>
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
#if DEBUG
            .LogToTrace()
#else
            .LogToTrace(Avalonia.Logging.LogEventLevel.Warning)
#endif
            .UseReactiveUI(_ => { });
    }

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
                var versionAttributes = Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
                if (versionAttributes.Length > 0)
                {
                    var informationalVersionAttribute =
                        (AssemblyInformationalVersionAttribute)versionAttributes[0];
                    currentVersion = new Version(informationalVersionAttribute.InformationalVersion);
                }
                else
                {
                    currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                }
            }
            catch
            {
                /* ignored */
            }

            field = currentVersion?.ToString() ?? "2.0.0";
            return field;
        }
    } = string.Empty;

    public static string Branch
    {
        get
        {
#if DEBUG
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

            var buildDateAttributes = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false);
            foreach (var attribute in buildDateAttributes)
                if (attribute is AssemblyMetadataAttribute { Key: "BuildDate" } metadata)
                {
                    _buidDate = metadata.Value ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
                    return _buidDate;
                }

            _buidDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            return _buidDate;
        }
    }

    #endregion
}

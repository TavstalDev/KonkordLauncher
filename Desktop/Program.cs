using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI.Avalonia;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Common.Services.Implementations;
using Tavstal.KonkordLauncher.Core.Encryption;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Services.Abstractions.Auth;
using Tavstal.KonkordLauncher.Core.Services.Implementations;
using Tavstal.KonkordLauncher.Core.Services.Implementations.Auth;
using Velopack;

namespace Tavstal.KonkordLauncher.Desktop;

// ReSharper disable once ClassNeverInstantiated.Global
/// <summary>
/// Entry point for the desktop application.
/// </summary>
class Program
{
    private static IServiceProvider? _serviceProvider;
    public static IServiceProvider ServiceProvider => _serviceProvider ?? throw new NullReferenceException();
    private static readonly CancellationTokenSource LogCts = new();
    
    /// <summary>
    /// Application entry point.
    /// 
    /// High-level flow:
    /// <br/>1. Run Velopack app build (application packaging/initialization step).
    /// <br/>2. Start background task that processes the queued logs via <see cref="LoggerHelper.ProcessLogQueueAsync"/>.
    /// <br/>3. Create a generic host and configure dependency injection for application services (Minecraft, launcher, auth, etc.).
    /// <br/>4. Configure data protection to persist keys to the user's application data directory and register provider with <see cref="EncryptionUtility"/>.
    /// <br/>5. Start the Avalonia classic desktop lifetime using <see cref="BuildAvaloniaApp"/>.
    /// <br/>
    /// Notes:
    /// <br/>- Avoid using any Avalonia or SynchronizationContext-dependent APIs before this method runs.
    /// <br/>- Services registered here are available application-wide via <see cref="ServiceProvider"/>.
    /// </summary>
    /// <param name="args">Command-line arguments forwarded to Avalonia application lifetime.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();
        
        _ = Task.Run(() => LoggerHelper.ProcessLogQueueAsync(LogCts.Token));
        
        var appHost = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton(typeof(ICustomLogger<>), typeof(CustomLogger<>));
                services.AddSingleton<IHttpService, HttpService>();
                
                // Minecraft services
                services.AddSingleton<IManifestService, ManifestService>();
                services.AddScoped<ILibraryDownloadService, LibraryDownloadService>();
                services.AddSingleton<IInstanceInstallService, InstanceInstallService>();
                services.AddSingleton<IInstanceLaunchService, InstanceLaunchService>();
                
                services.AddSingleton<ISkinService, SkinService>();
                services.AddSingleton<IMojangSkinService, MojangSkinService>();
                services.AddSingleton<IMicrosoftAuthService, MicrosoftAuthService>();
                services.AddSingleton<IMicrosoftDeviceAuthService, MicrosoftDeviceAuthService>();
                services.AddSingleton<IMicrosoftHttpAuthService, MicrosoftHttpAuthService>();
                
                // Launcher services
                services.AddSingleton<IJavaService, JavaService>();
                services.AddSingleton<ILauncherStore, LauncherStore>();
                services.AddSingleton<ITranslationService, TranslationService>();
                services.AddHostedService(sp => sp.GetRequiredService<TranslationService>());
                services.AddSingleton<IValidationService, ValidationService>();
                services.AddSingleton<IModrinthApiClient, ModrinthApiClient>();
                services.AddSingleton<ModrinthPackageService, ModrinthPackageService>();
                services.AddSingleton<CurseForgePackageService, CurseForgePackageService>();
                services.AddSingleton<IMetaCacheService, MetaCacheService>();
                
                var keyDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "KonkordLauncher",
                    "keys"
                );
                Directory.CreateDirectory(keyDir);
                services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(keyDir))
                    .SetApplicationName("KonkordLauncher");
            })
            .Build();
        
        // Get protector immediately
        _serviceProvider = appHost.Services;
        var provider = appHost.Services.GetRequiredService<IDataProtectionProvider>();
        EncryptionUtility.SetDataProtectionProvider(provider);

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Configures and returns the Avalonia <see cref="AppBuilder"/> used to start the UI.
    /// This method is intentionally static and called by the runtime; it is also used by the visual designer.
    /// </summary>
    /// <returns>A configured <see cref="AppBuilder"/> instance for Avalonia.</returns>
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI(_ => { });
}
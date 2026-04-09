using System;
using System.IO;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tavstal.KonkordLauncher.Core.Encryption;
using Velopack;

namespace Tavstal.KonkordLauncher.Desktop;

// ReSharper disable once ClassNeverInstantiated.Global
class Program
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static IHost AppHost { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static IServiceProvider Services => AppHost.Services;
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();
        AppHost = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
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
        var provider = AppHost.Services.GetRequiredService<IDataProtectionProvider>();
        EncryptionUtility.SetDataProtectionProvider(provider);

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
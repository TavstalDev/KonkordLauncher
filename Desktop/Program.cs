using System;
using System.IO;
using Avalonia;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI.Avalonia;
using Tavstal.KonkordLauncher.Core.Encryption;
using Velopack;

namespace Tavstal.KonkordLauncher.Desktop;

// ReSharper disable once ClassNeverInstantiated.Global
class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();
        var appHost = Host.CreateDefaultBuilder(args)
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
        var provider = appHost.Services.GetRequiredService<IDataProtectionProvider>();
        EncryptionUtility.SetDataProtectionProvider(provider);

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI(_ => { });
}
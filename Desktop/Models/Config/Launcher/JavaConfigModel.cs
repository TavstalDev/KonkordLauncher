using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

/// <summary>
///     Holds the Java settings for the launcher.
/// </summary>
public partial class JavaConfigModel : ObservableObject
{
    /// <summary>
    ///     Creates a new instance with default settings.
    /// </summary>
    public JavaConfigModel() { }

    /// <summary>
    ///     Creates a new instance with the given settings.
    /// </summary>
    /// <param name="minMemory">The minimum memory in megabytes.</param>
    /// <param name="maxMemory">The maximum memory in megabytes.</param>
    /// <param name="permaGen">The permanent generation memory size in megabytes.</param>
    /// <param name="defaultJavaPath">The default path to the Java executable.</param>
    /// <param name="jvmArguments">The JVM arguments used when starting Java.</param>
    public JavaConfigModel(uint minMemory, uint maxMemory, uint permaGen, string defaultJavaPath, string jvmArguments)
    {
        MinMemory = minMemory;
        MaxMemory = maxMemory;
        PermaGen = permaGen;
        DefaultJavaPath = defaultJavaPath;
        JvmArguments = jvmArguments;
    }

    /// <summary>
    ///     Gets or sets the minimum memory in megabytes.
    /// </summary>
    [ObservableProperty]
    public partial uint MinMemory { get; set; }

    /// <summary>
    ///     Gets or sets the maximum memory in megabytes.
    /// </summary>
    [ObservableProperty]
    public partial uint MaxMemory { get; set; }

    /// <summary>
    ///     Gets or sets the permanent generation memory size in megabytes.
    /// </summary>
    [ObservableProperty]
    public partial uint PermaGen { get; set; }

    /// <summary>
    ///     Gets or sets the default path to the Java executable.
    /// </summary>
    [ObservableProperty]
    public partial string DefaultJavaPath { get; set; }

    /// <summary>
    ///     Gets or sets the JVM arguments used when starting Java.
    /// </summary>
    [ObservableProperty]
    public partial string JvmArguments { get; set; }
}

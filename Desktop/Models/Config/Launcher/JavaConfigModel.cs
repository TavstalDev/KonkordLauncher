using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

/// <summary>
/// Represents the configuration model for Java settings in the launcher.
/// </summary>
public partial class JavaConfigModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the minimum memory allocation for Java in megabytes.
    /// </summary>
    [ObservableProperty]
    public partial uint MinMemory { get; set; }

    /// <summary>
    /// Gets or sets the maximum memory allocation for Java in megabytes.
    /// </summary>
    [ObservableProperty]
    public partial uint MaxMemory { get; set; }

    /// <summary>
    /// Gets or sets the permanent generation memory size for Java in megabytes.
    /// </summary>
    [ObservableProperty]
    public partial uint PermaGen { get; set; }

    /// <summary>
    /// Gets or sets the default file path to the Java executable.
    /// </summary>
    [ObservableProperty]
    public partial string DefaultJavaPath { get; set; }

    /// <summary>
    /// Gets or sets the JVM arguments to be used when launching Java.
    /// </summary>
    [ObservableProperty]
    public partial string JvmArguments { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaConfigModel"/> class with default values.
    /// </summary>
    public JavaConfigModel() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaConfigModel"/> class with specified values.
    /// </summary>
    /// <param name="minMemory">The minimum memory allocation for Java in megabytes.</param>
    /// <param name="maxMemory">The maximum memory allocation for Java in megabytes.</param>
    /// <param name="permaGen">The permanent generation memory size for Java in megabytes.</param>
    /// <param name="defaultJavaPath">The default file path to the Java executable.</param>
    /// <param name="jvmArguments">The JVM arguments to be used when launching Java.</param>
    public JavaConfigModel(uint minMemory, uint maxMemory, uint permaGen, string defaultJavaPath, string jvmArguments)
    {
        MinMemory = minMemory;
        MaxMemory = maxMemory;
        PermaGen = permaGen;
        DefaultJavaPath = defaultJavaPath;
        JvmArguments = jvmArguments;
    }
}
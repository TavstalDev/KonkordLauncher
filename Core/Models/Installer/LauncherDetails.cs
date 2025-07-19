namespace Tavstal.KonkordLauncher.Core.Models.Installer;

/// <summary>
/// Represents the details of a launcher, including its name and version.
/// </summary>
public class LauncherDetails
{
    /// <summary>
    /// Gets or sets the name of the launcher.
    /// </summary>
    public string LauncherName { get; set; }

    /// <summary>
    /// Gets or sets the version of the launcher.
    /// </summary>
    public string LauncherVersion { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LauncherDetails"/> class with the specified name and version.
    /// </summary>
    /// <param name="launcherName">The name of the launcher.</param>
    /// <param name="launcherVersion">The version of the launcher.</param>
    public LauncherDetails(string launcherName, string launcherVersion)
    {
        LauncherName = launcherName;
        LauncherVersion = launcherVersion;
    }
}
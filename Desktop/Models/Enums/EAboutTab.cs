namespace Tavstal.KonkordLauncher.Desktop.Models.Enums;

/// <summary>
/// Represents the tabs available in the About dialog/window of the application.
/// </summary>
public enum EAboutTab
{
    /// <summary>
    /// The main "About" tab which typically contains application description, version and build information.
    /// </summary>
    ABOUT = 0,
    
    /// <summary>
    /// The "Credits" tab which lists project maintainers, contributors, translators and testers.
    /// </summary>
    CREDITS = 1,
    
    /// <summary>
    /// The "License" tab which displays licensing information for the project and third-party components.
    /// </summary>
    LICENSE = 2
}
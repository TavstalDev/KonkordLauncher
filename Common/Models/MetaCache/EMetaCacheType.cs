namespace Tavstal.KonkordLauncher.Common.Models.MetaCache;

/// <summary>
/// Represents the type of metadata cached by the launcher.
/// </summary>
public enum EMetaCacheType
{
    /// <summary>
    /// Cached image data.
    /// </summary>
    IMAGE = 0,
    
    /// <summary>
    /// Cached search result data.
    /// </summary>
    SEARCH_RESULT = 1,
    
    /// <summary>
    /// Cached project metadata.
    /// </summary>
    PROJECT = 2,
    
    /// <summary>
    /// Cached version metadata.
    /// </summary>
    VERSION = 3
}
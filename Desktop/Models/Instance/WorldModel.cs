using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Helpers;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

/// <summary>
/// Represents a model for a Minecraft world, including its metadata and utility methods.
/// </summary>
public partial class WorldModel : ObservableObject
{
    /// <summary>
    /// The name of the world.
    /// </summary>
    [ObservableProperty] private string _name;

    /// <summary>
    /// The file path of the world.
    /// </summary>
    [ObservableProperty] private string _path;
    
    /// <summary>
    /// The game mode of the world (e.g., Survival, Creative).
    /// </summary>
    [ObservableProperty] private string _gamemode;

    /// <summary>
    /// The seed value used to generate the world.
    /// </summary>
    [ObservableProperty] public long _seed;
    
    /// <summary>
    /// The last played timestamp of the world in milliseconds since the Unix epoch.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedLastPlayed))]
    private long _lastPlayed;

    /// <summary>
    /// The size of the world in bytes.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedSize))]
    private long _size;

    /// <summary>
    /// The icon representing the world.
    /// </summary>
    [ObservableProperty] private Bitmap? _icon;

    /// <summary>
    /// Gets the formatted size of the world as a human-readable string.
    /// </summary>
    public string FormattedSize => FileSystemHelper.GetFormatedSize(Size);

    /// <summary>
    /// Gets the formatted last played time of the world as a human-readable string.
    /// </summary>
    public string FormattedLastPlayed
    {
        get
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            epoch = epoch.AddMilliseconds(LastPlayed);
            var timeDiff = DateTime.UtcNow - epoch;

            if (timeDiff.TotalHours < 1)
                return TranslationManager.Translate("common.time.pass.minute", timeDiff.Minutes);

            if (timeDiff.TotalDays < 1)
                return TranslationManager.Translate("common.time.pass.hour", $"{timeDiff.Hours}");

            if (timeDiff.TotalDays < 30)
                return TranslationManager.Translate("common.time.pass.day", $"{timeDiff.Days}");

            if (timeDiff.TotalDays < 365)
                return TranslationManager.Translate("common.time.pass.month",  $"{timeDiff.TotalDays / 30:F0}");

            return TranslationManager.Translate("common.time.pass.year", $"{timeDiff.TotalDays / 365:F0}");
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldModel"/> class with the specified properties.
    /// </summary>
    /// <param name="name">The name of the world.</param>
    /// <param name="path">The file path of the world.</param>
    /// <param name="gamemode">The game mode of the world (e.g., Survival, Creative).</param>
    /// <param name="seed">The seed value used to generate the world.</param>
    /// <param name="lastPlayed">The last played timestamp of the world in milliseconds since the Unix epoch.</param>
    /// <param name="size">The size of the world in bytes.</param>
    /// <param name="icon">The icon representing the world.</param>
    public WorldModel(string name, string path, string gamemode, long seed, long lastPlayed, long size, Bitmap? icon)
    {
        _name = name;
        _path = path;
        _gamemode = gamemode;
        _seed = seed;
        _lastPlayed = lastPlayed;
        _size = size;
        _icon = icon;
    }
}
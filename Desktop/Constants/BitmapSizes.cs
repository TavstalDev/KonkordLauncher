namespace Tavstal.KonkordLauncher.Desktop.Constants;

public static class BitmapSizes
{
    /// <summary>
    ///     Maximum width used by an instance icon (instance list and play card).
    /// </summary>
    public const int InstanceModel = 72;

    /// <summary>
    ///     Maximum width used by the icons in the icon selector. The same files are also used as
    ///     instance icons, so keep the larger instance width for the shared bitmap cache.
    /// </summary>
    public const int IconSelectorIcon = InstanceModel;

    /// <summary>
    ///     Maximum width used by Modrinth project icons in the modpack and resource search lists.
    /// </summary>
    public const int ProjectIcon = 35;

    /// <summary>
    ///     Maximum width used by mod icons in the mods table.
    /// </summary>
    public const int ModIcon = 32;

    /// <summary>
    ///     Maximum width used by resource pack icons in the resource packs table.
    /// </summary>
    public const int ResourcePackIcon = 32;

    /// <summary>
    ///     Maximum width used by shader pack icons (same table layout as the other resources).
    /// </summary>
    public const int ShaderPackIcon = 32;

    /// <summary>
    ///     Maximum width used by world icons in the worlds table.
    /// </summary>
    public const int WorldIcon = 48;

    /// <summary>
    ///     Maximum width used by server icons in the servers table.
    /// </summary>
    public const int ServerIcon = 48;

    /// <summary>
    ///     Maximum width used by the screenshot preview in the screenshots table.
    /// </summary>
    public const int ScreenshotIcon = 48;

    /// <summary>
    ///     Maximum width of the fallback world image. It is shared by the worlds, servers,
    ///     resource packs and shader packs tables, so it uses the largest of those widths.
    /// </summary>
    public const int DefaultWorldIcon = WorldIcon;

    /// <summary>
    ///     Maximum width of the account avatar in the sidebar (75px panel with 10px margins).
    /// </summary>
    public const int AccountAvatar = 55;

    /// <summary>
    ///     Maximum width of the 2D skin preview in the download-size skin Viewbox (280px high).
    /// </summary>
    public const int SkinPreview = 138;

    /// <summary>
    ///     Maximum width of a cape in the cape card Viewbox (70px high).
    /// </summary>
    public const int CapePreview = 86;
}

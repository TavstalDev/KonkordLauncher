using System;
using Avalonia.Controls;
using Tavstal.BeaconSDK.Models;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;

namespace Tavstal.KonkordLauncher.Desktop.Translation;

public static class TranslationHelper
{
    public static ITranslationService? Current
    {
        get
        {
            if (Design.IsDesignMode)
                field ??= new DesignTimeTranslationService();
            return field;
        }
        set;
    }

    public static string Translate(string key)
    {
        return Current?.Translate(key) ?? key;
    }

    public static string Translate(string key, params object[]? args)
    {
        return Current?.Translate(key, args) ?? key;
    }

    public static string GetProgressKey(EProgressKind progressKind)
    {
        return progressKind switch
        {
            // Reading
            EProgressKind.INSTANCE_READING_MANIFEST => "instance.reading.manifest",
            EProgressKind.INSTANCE_READING_VERSION_JSON => "instance.reading.version_json",
            EProgressKind.INSTANCE_READING_VERSION_JAR => "instance.reading.version_jar",
            EProgressKind.INSTANCE_READING_ASSET_INDEX_JSON => "instance.reading.asset_index_json",
            EProgressKind.INSTANCE_READING_LOGGING => "instance.reading.logging",
            EProgressKind.INSTANCE_READING_CLIENT_MAPPINGS => "instance.reading.client_mappings",
            EProgressKind.INSTANCE_READING_ASSETS => "instance.reading.assets",
            EProgressKind.INSTANCE_READING_LIBRARIES => "instance.reading.libraries",
            // Downloading
            EProgressKind.INSTANCE_DOWNLOADING_VERSION_JSON => "instance.downloading.version_json",
            EProgressKind.INSTANCE_DOWNLOADING_VERSION_JAR => "instance.downloading.version_jar",
            EProgressKind.INSTANCE_DOWNLOADING_ASSET_INDEX_JSON => "instance.downloading.asset_index_json",
            EProgressKind.INSTANCE_DOWNLOADING_LOGGING => "instance.downloading.logging",
            EProgressKind.INSTANCE_DOWNLOADING_CLIENT_MAPPINGS => "instance.downloading.client_mappings",
            EProgressKind.INSTANCE_DOWNLOADING_FABRIC_LOADER => "instance.downloading.loader",
            EProgressKind.INSTANCE_DOWNLOADING_ASSETS => "instance.downloading.assets",
            EProgressKind.INSTANCE_DOWNLOADING_LIBRARIES => "instance.downloading.libraries",
            EProgressKind.INSTANCE_DOWNLOADING_NATIVES => "instance.downloading.natives",
            EProgressKind.INSTANCE_DOWNLOADING_INSTALLER => "instance.downloading.installer",
            EProgressKind.INSTANCE_DOWNLOAD_FILE => "instance.download.file",
            // Extracting / Building
            EProgressKind.INSTANCE_EXTRACTING_INSTALLER => "instance.extracting.installer",
            EProgressKind.INSTANCE_BUILDING => "instance.building.progress",
            EProgressKind.INSTANCE_BUILDING_ARGUMENTS => "instance.building.arguments",
            // Auth
            EProgressKind.AUTH_MICROSOFT_AUTHENTICATING => "auth.microsoft.authenticating",
            EProgressKind.AUTH_CODE_CREATING => "auth.code.creating",
            EProgressKind.AUTH_XBOX_AUTHENTICATING => "auth.xbox.authenticating",
            EProgressKind.AUTH_XBOX_XSTS => "auth.xbox.xsts",
            EProgressKind.AUTH_MINECRAFT_AUTHENTICATING => "auth.minecraft.authenticating",
            EProgressKind.AUTH_MINECRAFT_OWNERSHIP => "auth.minecraft.ownership",
            EProgressKind.AUTH_MINECRAFT_PROFILE => "auth.minecraft.profile",
            EProgressKind.AUTH_LISTENER_STARTING => "auth.listener.starting",
            EProgressKind.AUTH_LISTENER_FAILED => "auth.listener.failed",
            EProgressKind.AUTH_LISTENER_CANCELLED => "auth.listener.cancelled",
            EProgressKind.AUTH_LISTENER_CALLBACK => "auth.listener.callback",
            _ => throw new ArgumentOutOfRangeException(nameof(progressKind), progressKind, null)
        };
    }
}

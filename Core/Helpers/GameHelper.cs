using System.Text;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models.Launcher;
using Tavstal.KonkordLauncher.Core.Models.Minecraft;

namespace Tavstal.KonkordLauncher.Core.Helpers;

public static class GameHelper
{
    public static VersionDetails GetProfileVersionDetails(string versionsDir, EProfileKind kind, string versionId,
        string? vanillaVersionId = null, string? customDirectory = null)
    {
        string vanillaVersion = vanillaVersionId ?? versionId;
        VersionDetails response = new VersionDetails
        {
            InstanceVersion = versionId,
            VanillaVersion = vanillaVersion
        };

        string versionName = $"{response.VanillaVersion}";
        switch (kind)
        {
            case EProfileKind.FORGE:
            {
                versionName = $"{vanillaVersionId}-forge-{versionId}";
                break;
            }
            case EProfileKind.FABRIC:
            {
                versionName = $"{vanillaVersionId}-fabric-{versionId}";
                break;
            }
            case EProfileKind.QUILT:
            {
                versionName = $"{vanillaVersionId}-quilt-{versionId}";
                break;
            }
        }

        response.VersionDirectory = Path.Combine(versionsDir, versionName);
        response.VersionJsonPath = Path.Combine(response.VersionDirectory, $"{versionName}.json");
        response.VersionJarPath = Path.Combine(response.VersionDirectory, $"{versionName}.jar");
        response.VanillaJarPath = Path.Combine(PathHelper.VersionsDir, vanillaVersion, $"{vanillaVersionId}.jar");
        response.NativesDir = Path.Combine(response.VersionDirectory, "natives");
        if (string.IsNullOrEmpty(customDirectory))
            response.GameDir = Path.Combine(PathHelper.InstancesDir, versionName);
        else
            response.GameDir = customDirectory;

        return response;
    }

    /// <summary>
    /// Gets the version details for a profile based on the specified parameters.
    /// </summary>
    /// <param name="type">The type of profile.</param>
    /// <param name="manifest">Optional: The version manifest.</param>
    /// <param name="profile">Optional: The profile.</param>
    /// <returns>
    /// The version details.
    /// </returns>
    public static VersionDetails GetProfileVersionDetails(EProfileType type, VersionManifest? manifest = null,
        Profile? profile = null)
    {
        switch (type)
        {
            case EProfileType.LATEST_RELEASE:
            {
                if (manifest == null)
                    throw new ArgumentNullException(nameof(manifest));

                return GetProfileVersionDetails(EProfileKind.VANILLA, manifest.Latest.Release, manifest.Latest.Release,
                    null);
            }
            case EProfileType.LATEST_SNAPSHOT:
            {
                if (manifest == null)
                    throw new ArgumentNullException(nameof(manifest));

                return GetProfileVersionDetails(EProfileKind.VANILLA, manifest.Latest.Snapshot,
                    manifest.Latest.Snapshot, null);
            }
            case EProfileType.CUSTOM:
            {
                if (profile == null)
                    throw new ArgumentNullException(nameof(profile));


                return GetProfileVersionDetails(profile.Kind, profile.VersionId, profile.VersionVanillaId,
                    profile.GameDirectory);
            }
            default:
            {
                throw new NotImplementedException("How did we get here ?");
            }
        }
    }
    
    /// <summary>
    /// Retrieves the UUID (Universally Unique Identifier) of a player based on the provided username.
    /// </summary>
    /// <param name="username">The username of the player.</param>
    /// <returns>
    /// A <see cref="string"/> representing the UUID of the player.
    /// </returns>
    private static string GetPlayerUUID(string username)
    {
        //new GameProfile(UUID.nameUUIDFromBytes(("OfflinePlayer:" + name).getBytes(Charsets.UTF_8)), name));
        byte[] rawresult = System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(username));
        //set the version to 3 -> Name based md5 hash
        rawresult[6] = (byte)(rawresult[6] & 0x0f | 0x30);
        //IETF variant
        rawresult[8] = (byte)(rawresult[8] & 0x3f | 0x80);
        //convert to string and remove any - if any
        string finalresult = BitConverter.ToString(rawresult).Replace("-", "");
        //formatting
        finalresult = finalresult.Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
        return finalresult;
    }

    /// <summary>
    /// Retrieves the UUID (Universally Unique Identifier) of an offline player based on the provided username.
    /// </summary>
    /// <param name="username">The username of the offline player.</param>
    /// <returns>
    /// A <see cref="string"/> representing the UUID of the offline player.
    /// </returns>
    public static string GetOfflinePlayerUUID(string username)
    {
        return GetPlayerUUID($"OfflinePlayer:{username}");
    }

    /// <summary>
    /// Retrieves the UUID (Universally Unique Identifier) of an online player based on the provided username.
    /// </summary>
    /// <param name="username">The username of the online player.</param>
    /// <returns>
    /// A <see cref="string"/> representing the UUID of the online player.
    /// </returns>
    public static string GetOnlinePlayerUUID(string username)
    {
        return GetPlayerUUID($"{username}");
    }
}
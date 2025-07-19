using System.Text;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Helpers;

public static class GameHelper
{
    /// <summary>
    /// Retrieves the version details for a specific Minecraft version based on the provided parameters.
    /// </summary>
    /// <param name="versionsDir">The directory where version files are stored.</param>
    /// <param name="minecraftVersion">The Minecraft version identifier.</param>
    /// <param name="kind">The kind of profile (e.g., Forge, Fabric, Quilt).</param>
    /// <param name="customVersion">
    /// Optional: A custom version identifier. If not provided, the Minecraft version will be used.
    /// </param>
    /// <param name="customDirectory">
    /// Optional: A custom directory for the game files. If not provided, a default directory will be used.
    /// </param>
    /// <returns>
    /// A <see cref="VersionDetails"/> object containing the details of the specified Minecraft version.
    /// </returns>
    public static VersionDetails GetVersionDetails(string versionsDir, string minecraftVersion, EMinecraftKind kind,
        string? customVersion = null, string? customDirectory = null)
    {
        // Initialize the response object with the custom and Minecraft version details
        VersionDetails response = new VersionDetails
        {
            CustomVersion = customVersion ?? minecraftVersion,
            MinecraftVersion = minecraftVersion
        };

        // Construct the version name based on the profile kind
        string versionName = $"{response.MinecraftVersion}";
        switch (kind)
        {
            case EMinecraftKind.FORGE:
            {
                versionName = $"{minecraftVersion}-forge-{customVersion}";
                break;
            }
            case EMinecraftKind.FABRIC:
            {
                versionName = $"{minecraftVersion}-fabric-{customVersion}";
                break;
            }
            case EMinecraftKind.QUILT:
            {
                versionName = $"{minecraftVersion}-quilt-{customVersion}";
                break;
            }
        }

        // Set the paths for various version-related files and directories
        var versionDir = Path.Combine(versionsDir, versionName);
        response.VersionDirectory = versionDir;
        response.VersionJsonPath = Path.Combine(versionDir, $"{versionName}.json");
        response.VersionJarPath = Path.Combine(versionDir, $"{versionName}.jar");
        response.NativesDir = Path.Combine(versionDir, "natives");

        // Set the path to the vanilla JAR file
        response.VanillaJarPath = Path.Combine(versionsDir, minecraftVersion, $"{minecraftVersion}.jar");

        // Determine the game directory, using the custom directory if provided
        response.GameDir = string.IsNullOrEmpty(customDirectory)
            ? Path.Combine(versionsDir, versionName)
            : customDirectory;

        // Return the constructed version details
        return response;
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
}
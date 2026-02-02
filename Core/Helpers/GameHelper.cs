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
            case EMinecraftKind.NEOFORGE:
            {
                versionName = $"{minecraftVersion}-neoforge-{customVersion}";
                break;
            }
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

        // Set the path to the vanilla JAR file
        response.VanillaJarPath = Path.Combine(versionsDir, minecraftVersion, $"{minecraftVersion}.jar");

        // Determine the game directory, using the custom directory if provided
        response.GameDir = string.IsNullOrEmpty(customDirectory)
            ? Path.Combine(versionsDir, versionName)
            : customDirectory;
        
        response.NativesDir = Path.Combine(response.GameDir, "natives");

        // Return the constructed version details
        return response;
    }

    /// <summary>
    /// Generates an offline UUID for a player based on their username.
    /// </summary>
    /// <param name="username">The username of the player.</param>
    /// <returns>
    /// A string representing the offline UUID in lowercase hexadecimal format without dashes.
    /// </returns>
    /// <remarks>
    /// The UUID is generated using the MD5 hash of the string "OfflinePlayer:{username}".
    /// The UUID version is set to 3 (name-based MD5), and the IETF variant is applied.
    /// </remarks>
    public static string GetOfflinePlayerUUID(string username)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        
        byte[] hash = md5.ComputeHash(
            Encoding.UTF8.GetBytes($"OfflinePlayer:{username}")
        );

        // Set UUID version to 3 (name-based MD5)
        hash[6] = (byte)((hash[6] & 0x0F) | 0x30);

        // Set IETF variant
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        // Convert to lowercase hex, no dashes
        return BitConverter.ToString(hash)
            .Replace("-", "")
            .ToLowerInvariant();
    }
}
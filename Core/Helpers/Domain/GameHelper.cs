using System.Security.Cryptography;
using System.Text;

namespace Tavstal.KonkordLauncher.Core.Helpers.Domain;

/// <summary>
/// Provides utility helper methods for game-related operations, including version comparison and UUID generation.
/// </summary>
public static class GameHelper
{
    /// <summary>
    /// Compares two version strings to determine if the first version is newer than the second.
    /// </summary>
    /// <param name="versionA">The first version string to compare.</param>
    /// <param name="versionB">The second version string to compare.</param>
    /// <returns><c>true</c> if <paramref name="versionA"/> is newer than <paramref name="versionB"/>; otherwise, <c>false</c>.</returns>
    public static bool isNewer(string versionA, string versionB)
    {
        int[] verA = ParseVersionString(versionA);
        int[] verB = ParseVersionString(versionB);

        int length = Math.Max(verA.Length, verB.Length);
        for (int i = 0; i < length; i++)
        {
            int partA = i < verA.Length ? verA[i] : 0;
            int partB = i < verB.Length ? verB[i] : 0;

            if (partA > partB)
                return true;
            if (partA < partB)
                return false;
        }
        return false; // Versions are equal
    }

    /// <summary>
    /// Parses a version string into an array of integers.
    /// </summary>
    /// <param name="version">The version string to parse (e.g., "1.2.3").</param>
    /// <returns>An array of integers representing the version parts. If a part cannot be parsed, it defaults to 0.</returns>
    public static int[] ParseVersionString(string version)
    {
        var parts = version.Split('.');
        var versionNumbers = new int[parts.Length];

        for (int i = 0; i < parts.Length; i++)
        {
            if (int.TryParse(parts[i], out int number))
                versionNumbers[i] = number;
            else
                versionNumbers[i] = 0; // Default to 0 if parsing fails
        }

        return versionNumbers;
    }

    /// <summary>
    /// Retrieves the UUID (Universally Unique Identifier) of an offline player based on the provided username.
    /// </summary>
    /// <param name="username">The username of the offline player.</param>
    /// <returns>A <see cref="string"/> representing the UUID of the offline player.</returns>
    public static string GetOfflinePlayerUUID(string username) => GetPlayerUUID($"OfflinePlayer:{username}");
    
    /// <summary>
    /// Retrieves the UUID (Universally Unique Identifier) of a player based on the provided username.
    /// </summary>
    /// <param name="username">The username of the player.</param>
    /// <returns>A <see cref="string"/> representing the UUID of the player.</returns>
    private static string GetPlayerUUID(string username)
    {
        byte[] rawresult = MD5.HashData(Encoding.UTF8.GetBytes(username));
        //set the version to 3 -> Name based md5 hash
        rawresult[6] = (byte)(rawresult[6] & 0x0f | 0x30);
        //IETF variant
        rawresult[8] = (byte)(rawresult[8] & 0x3f | 0x80);
        //convert to string and remove any - if any
        string finalresult = Convert.ToHexString(rawresult).Replace("-", "");
        //formatting
        finalresult = finalresult
            .Insert(8, "-")
            .Insert(13, "-")
            .Insert(18, "-")
            .Insert(23, "-");
        return finalresult;
    }
}
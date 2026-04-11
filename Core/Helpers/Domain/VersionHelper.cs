namespace Tavstal.KonkordLauncher.Core.Helpers.Domain;

/// <summary>
/// Provides helper methods for version comparison and parsing.
/// </summary>
public static class VersionHelper
{
    /// <summary>
    /// Compares two version strings to determine if the first version is newer than the second.
    /// </summary>
    /// <param name="versionA">The first version string to compare.</param>
    /// <param name="versionB">The second version string to compare.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="versionA"/> is newer than <paramref name="versionB"/>; otherwise, <c>false</c>.
    /// </returns>
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
    /// <returns>
    /// An array of integers representing the version parts. If a part cannot be parsed, it defaults to 0.
    /// </returns>
    public static int[] ParseVersionString(string version)
    {
        var parts = version.Split('.');
        var versionNumbers = new int[parts.Length];

        for (int i = 0; i < parts.Length; i++)
        {
            if (int.TryParse(parts[i], out int number))
            {
                versionNumbers[i] = number;
            }
            else
            {
                versionNumbers[i] = 0; // Default to 0 if parsing fails
            }
        }

        return versionNumbers;
    }
}
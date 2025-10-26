namespace Tavstal.KonkordLauncher.Core.Helpers;

public static class VersionHelper
{
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
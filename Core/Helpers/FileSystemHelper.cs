using System.Security.Cryptography;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Helpers;

/// <summary>
/// Provides helper methods for file system operations such as deleting, moving directories, and verifying file hashes.
/// </summary>
public static class FileSystemHelper
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(FileSystemHelper));

    /// <summary>
    /// Deletes a directory and all its contents.
    /// </summary>
    /// <param name="path">The path of the directory to delete.</param>
    public static void DeleteDirectory(string path)
    {
        var forgeInstallerDirInfo = new DirectoryInfo(path);
        foreach (FileInfo file in forgeInstallerDirInfo.GetFiles())
            file.Delete();

        foreach (DirectoryInfo subDirectory in forgeInstallerDirInfo.GetDirectories())
            subDirectory.Delete(true);

        Directory.Delete(path);
    }

    /// <summary>
    /// Moves a directory and its contents to a new location.
    /// </summary>
    /// <param name="sourceDir">The source directory path.</param>
    /// <param name="destinationDir">The destination directory path.</param>
    /// <param name="recursive">Indicates whether to move subdirectories recursively.</param>
    /// <param name="deleteSource">Indicates whether to delete the source directory after moving. Default is true.</param>
    /// <param name="overwrite">Indicates whether to overwrite existing files in the destination. Default is true.</param>
    /// <exception cref="DirectoryNotFoundException">Thrown if the source directory does not exist.</exception>
    public static void MoveDirectory(string sourceDir, string destinationDir, bool recursive, bool deleteSource = true,
        bool overwrite = true)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            if (overwrite || (!overwrite && !File.Exists(targetFilePath)))
                file.CopyTo(targetFilePath, true);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                MoveDirectory(subDir.FullName, newDestinationDir, true, false);
            }

        if (deleteSource)
            DeleteDirectory(sourceDir);
    }

    /// <summary>
    /// Verifies the SHA1 hash of a file against a given hash value.
    /// </summary>
    /// <param name="path">The path of the file to check.</param>
    /// <param name="compareHash">The SHA1 hash to compare against. If null or empty, the method returns true.</param>
    /// <returns>True if the file's hash matches the given hash; otherwise, false.</returns>
    public static bool CheckSHA1(string path, string? compareHash)
    {
        if (string.IsNullOrEmpty(compareHash))
            return true;

        try
        {
            string fileHash;
            using (FileStream file = File.OpenRead(path))
            using (SHA1 hasher = SHA1.Create())
            {
                var binaryHash = hasher.ComputeHash(file);
                fileHash = Convert.ToHexStringLower(binaryHash);
            }

            return string.Equals(fileHash, compareHash);
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to check SHA1 hash:");
            _logger.Error(ex.ToString());
            return false;
        }
    }
}
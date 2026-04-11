using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Helpers.IO;

/// <summary>
/// Provides helper methods for file system operations such as deleting, moving directories, and verifying file hashes.
/// </summary>
public static class FileSystemHelper
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(FileSystemHelper));

    #region Core Operations
    
    /// <summary>
    /// Deletes the file at the provided path if it exists, is not locked, and passes safety checks.
    /// </summary>
    /// <param name="path">Full path to the file to delete.</param>
    public static bool DeleteFile(string path)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
            return false;
        
        if (!File.Exists(path))
            return true; // Act like the file was deleted

        if (!MakeFileWritable(path))
        {
            _logger.Error($"Failed to clear read-only attribute from file '{path}' before deletion.");
            return false;
        }
        
        if (IsFileLocked(path))
        {
            _logger.Error($"Refusing to delete file '{path}' because it is currently locked by another process.");
            return false;
        }

        if (!IsSafeToDelete(path))
        {
            _logger.Error("Refusing to delete file '{path}' because it is not considered safe to delete.");
            return false;
        }

        try
        {
            File.Delete(path);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while trying to delete file '{path}': {ex}");
            return false;
        }
    }
    
    /// <summary>
    /// Deletes a directory and all its contents after verifying the directory is safe to delete.
    /// </summary>
    /// <param name="path">The path of the directory to delete.</param>
    public static bool DeleteDirectory(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
                return false;
            
            if (!Directory.Exists(path))
                return true;

            if (!IsSafeToDelete(path))
            {
                _logger.Error($"Refusing to delete directory '{path}' because it is not considered safe to delete.");
                return false;
            }

            string homeDir = OSHelper.GetHomeDirectory();
            if (string.Equals(path, homeDir, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Error($"Refusing to delete directory '{path}' because it is the user's home directory.");
                return false;
            }

            string programsDir = OSHelper.GetProgramsDirectory();
            if (string.Equals(path, programsDir, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Error($"Refusing to delete directory '{path}' because it is the system's programs directory.");
                return false;
            }

            string desktopDir = OSHelper.GetDesktopDirectory();
            if (string.Equals(path, desktopDir, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Error($"Refusing to delete directory '{path}' because it is the user's desktop directory.");
                return false;
            }

            var dirInfo = new DirectoryInfo(path);
            // Delete files
            var files = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                if (!DeleteFile(file.FullName))
                {
                    _logger.Error($"Failed to delete file '{file.FullName}' while deleting directory '{path}'. Aborting directory deletion.");
                    return false;
                }
            }

            // Delete sub dirs
            var dirs = Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories)
                .OrderByDescending(d => d.Length)
                .ToList();
            foreach (string dir in dirs)
            {
                try
                {
                    var subDir =  new DirectoryInfo(dir);
                    var attrs = subDir.Attributes;
                    if (attrs.HasFlag(FileAttributes.ReadOnly))
                        dirInfo.Attributes = attrs & ~FileAttributes.ReadOnly;
                    Directory.Delete(dir);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Unexpected error while trying to delete subdirectory '{dir}': {ex}");
                    return false;
                }
            }

            Directory.Delete(path);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while trying to delete directory '{path}': {ex}");
            return false;
        }
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
    #endregion
    
    #region File Permissions and Attributes
    
    /// <summary>
    /// Makes a file executable by modifying its permissions using the `chmod` command.
    /// </summary>
    /// <param name="path">The path of the file to make executable.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a boolean value:
    /// true if the operation succeeded, false otherwise.
    /// </returns>
    public static async Task<bool> MakeExecutableAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return false;
            
            if (!File.Exists(path))
                return false;
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{path}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                string error = await process.StandardError.ReadToEndAsync();
                _logger.Exc($"Error while making '{path}' executable:");
                _logger.Error(error);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exc($"Failed to make '{path}' executable:");
            _logger.Error(ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// Sets the file at <paramref name="pathToFile"/> to read-only by adding the <see cref="FileAttributes.ReadOnly"/> flag.
    /// If the file does not exist or is already read-only the method returns immediately.
    /// </summary>
    /// <param name="pathToFile">Full path to the file to mark as read-only.</param>
    public static bool MakeFileReadonly(string pathToFile)
    {
        try
        {
            if (!File.Exists(pathToFile))
                return false;
            
            var attributes = File.GetAttributes(pathToFile);
            if (attributes.HasFlag(FileAttributes.ReadOnly))
                return true;
            
            attributes |= FileAttributes.ReadOnly;
            File.SetAttributes(pathToFile, attributes);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exc($"Failed to following file readonly {pathToFile}:");
            _logger.Error(ex);
            return false;
        }
    }
    
    /// <summary>
    /// Clears the read-only flag from the file at <paramref name="pathToFile"/>, making it writable.
    /// If the file does not exist or is not read-only the method returns immediately.
    /// </summary>
    /// <param name="pathToFile">Full path to the file to make writable.</param>
    public static bool MakeFileWritable(string pathToFile)
    {
        try
        {
            if (!File.Exists(pathToFile))
                return false;
            
            var attributes = File.GetAttributes(pathToFile);
            if (!attributes.HasFlag(FileAttributes.ReadOnly))
                return true;
            
            attributes &= ~FileAttributes.ReadOnly;
            File.SetAttributes(pathToFile, attributes);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exc($"Failed to following file writable {pathToFile}:");
            _logger.Error(ex);
            return false;
        }
    }
    
    /// <summary>
    /// Tests whether the application can write to the given directory by creating a small test file.
    /// Preserves the original behavior: creates the directory if needed, creates "test.txt", verifies its existence,
    /// deletes it if created, and returns true on success; logs and returns false on any exception.
    /// </summary>
    /// <param name="targetDir">Directory to test write permissions for.</param>
    /// <returns>True if a test file could be created and detected; otherwise false.</returns>
    public static bool HasWritePermission(string targetDir)
    {
        try
        {
            Directory.CreateDirectory(targetDir);
            
            string testFile = Path.Combine(targetDir, "test.txt");
            DeleteFile(testFile);
            
            File.WriteAllText(testFile, "test");
            bool success = File.Exists(testFile);
            if (success)
                DeleteFile(testFile);
            return success;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while testing write permissions at {targetDir}:");
            _logger.Error(ex);
            return false;
        }
    }
    
    /// <summary>
    /// Asynchronously tests whether the application has write permission to the given directory by attempting
    /// to create a small test file ("test.txt") inside it.
    /// </summary>
    /// <param name="targetDir">The directory to test write permissions for. The directory will be created if it does not exist.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is <c>true</c> if the test file could be created
    /// and detected; otherwise <c>false</c>. Any exception is logged and results in <c>false</c>.
    /// </returns>
    public static async Task<bool> HasWritePermissionAsync(string targetDir)
    {
        try
        {
            Directory.CreateDirectory(targetDir);
            
            string testFile = Path.Combine(targetDir, "test.txt");
            DeleteFile(testFile);
            
            await File.WriteAllTextAsync(testFile, "test");
            bool success = File.Exists(testFile);
            if (success)
                File.Delete(testFile);
            return success;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while testing write permissions at {targetDir}:");
            _logger.Error(ex);
            return false;
        }
    }
    
    /// <summary>
    /// Checks whether the drive containing <paramref name="targetDir"/> has at least <paramref name="bytes"/>
    /// bytes of available free space.
    /// </summary>
    /// <param name="targetDir">The directory to check the drive of. The method will determine the root drive from this path.</param>
    /// <param name="bytes">The minimum number of free bytes required.</param>
    public static bool HasEnoughFreeSpace(string targetDir, long bytes)
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(targetDir) ?? targetDir);
            return driveInfo.AvailableFreeSpace >= bytes;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while checking free space at {targetDir}:");
            _logger.Error(ex);
            return false;
        }
    }
    
    #endregion

    #region Hashing
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
    
    /// <summary>
    /// Verifies the SHA256 hash of a file against a given hash value.
    /// </summary>
    /// <param name="path">The path of the file to check.</param>
    /// <param name="compareHash">
    /// The SHA256 hash to compare against. If null or empty, the method returns true.
    /// </param>
    /// <returns>
    /// True if the file's hash matches the given hash; otherwise, false.
    /// </returns>
    public static bool CheckSHA256(string path, string? compareHash)
    {
        if (string.IsNullOrEmpty(compareHash))
            return true;

        try
        {
            string fileHash;
            using (FileStream file = File.OpenRead(path))
            using (SHA256 hasher = SHA256.Create())
            {
                var binaryHash = hasher.ComputeHash(file);
                fileHash = Convert.ToHexStringLower(binaryHash);
            }

            return string.Equals(fileHash, compareHash);
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to check SHA256 hash:");
            _logger.Error(ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// Verifies a file's hash using a digest string in the format "type:hash".
    /// Supported digest types are "sha1" and "sha256" (case-insensitive).
    /// </summary>
    /// <param name="path">Filesystem path to the file to verify.</param>
    /// <param name="digest">
    /// The digest string in the form "type:hash", for example "sha1:abcdef..." or "sha256:0123...".
    /// The method splits on the first ':' and treats the left part as the digest type and the right part as the hex hash.
    /// </param>
    /// <returns>
    /// <c>true</c> when the digest format is valid and the file's computed hash matches the provided hash;
    /// <c>false</c> when the digest format is invalid, the digest type is unsupported, or the hash comparison fails.
    /// </returns>
    public static bool CheckByDigest(string path, string digest)
    {
        string[] parts = digest.Split(':', 2);
        if (parts.Length < 2)
        {
            _logger.Error($"Invalid digest format: '{digest}'. Expected format is 'type:hash'.");
            return false;
        }
        
        switch (parts[0].ToLower())
        {
            case "sha1":
                return CheckSHA1(path, parts[1]);
            case "sha256":
                return CheckSHA256(path, parts[1]);
            default:
                _logger.Error($"Unsupported digest type '{parts[0]}' in digest string '{digest}'.");
                return false;
        }
    }

    /// <summary>
    /// Asynchronously computes the SHA-1 hash of the file at the given path and returns it as a lowercase hex string.
    /// </summary>
    /// <param name="path">Path to the file to hash.</param>
    public static async Task<string?> GetFileHashAsync(string path)
    {
        try
        {
            await using FileStream stream = File.OpenRead(path);
            return await GetFileHashAsync(stream);
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to compute file hash:");
            _logger.Error(ex.ToString());
            return null;
        }
    }
    
    /// <summary>
    /// Computes the SHA-1 hash of the provided stream and returns it as a lowercase hexadecimal string.
    /// </summary>
    /// <param name="stream">
    /// The input stream to hash. The method reads from the current stream position to the end.
    /// The caller is responsible for the stream's lifetime (this method does not dispose the provided stream).
    /// </param>
    public static async Task<string?> GetFileHashAsync(Stream stream)
    {
        try
        {
            using var sha = SHA1.Create();
            byte[] hashBytes = await sha.ComputeHashAsync(stream);
            string fileHash = Convert.ToHexStringLower(hashBytes);
            return fileHash;
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to compute file hash:");
            _logger.Error(ex.ToString());
            return null;
        }
    }
    
    /// <summary>
    /// Computes the SHA-1 hash of the provided string using UTF-8 encoding and returns it as a lowercase hexadecimal string.
    /// </summary>
    /// <param name="content">The input string to hash (encoded as UTF-8).</param>
    public static string? GetContentHash(string content)
    {
        try
        {
            using var sha = SHA1.Create();
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            using var stream = new MemoryStream(contentBytes);
            byte[] hashBytes = sha.ComputeHash(stream);
            string contentHash = Convert.ToHexStringLower(hashBytes);
            return contentHash;
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to compute content hash:");
            _logger.Error(ex.ToString());
            return null;
        }
    }
    #endregion
    
    #region Safety
    /// <summary>
    /// Checks whether the specified file is locked by attempting to open it with exclusive access.
    /// </summary>
    /// <param name="filePath">Full path to the file to test.</param>
    /// <returns>
    /// <c>true</c> if the file could not be opened due to an <see cref="IOException"/> (commonly indicates the file is locked);
    /// <c>false</c> if the file was successfully opened with exclusive access.
    /// </returns>
    public static bool IsFileLocked(string filePath)
    {
        if (!File.Exists(filePath))
            return false;
        
        try
        {
            using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
        catch (IOException)
        {
            return true;
        }
    }
    
    /// <summary>
    /// Determines whether the specified path is considered safe for deletion.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    /// <returns>
    /// <c>true</c> when the path appears valid and not obviously unsafe to delete (e.g. not null/empty,
    /// not a drive root and of reasonable length); otherwise <c>false</c>.
    /// </returns>
    public static bool IsSafeToDelete(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) 
            return false;
        var full = Path.GetFullPath(path);
        var root = Path.GetPathRoot(full);
        if (string.Equals(full, root, StringComparison.OrdinalIgnoreCase)) 
            return false;
        if (full.Length < 10) 
            return false;
        return true;
    }
    #endregion
    
    /// <summary>
    /// Opens a folder in the system's default file explorer.
    /// </summary>
    /// <param name="path">The path to the folder to open.</param>
    public static void OpenFolderInFileExplorer(string path)
    {
        // Ensure the path exists
        if (!Directory.Exists(path))
        {
            _logger.Error($"Error: Folder not found at '{path}'");
            return;
        }

        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                Process.Start("explorer.exe", path);
                break;
            }
            case EOperatingSystem.MacOS:
            {
                Process.Start("open", path);
                break;
            }
            case EOperatingSystem.Linux:
            {
                Process.Start("xdg-open", path);
                break;
            }
            case EOperatingSystem.Unknown:
            {
                _logger.Error("Error: Unsupported operating system for opening folder in file explorer.");
                break;
            }
        }
    }
    
    /// <summary>
    /// Converts a byte count into a human-readable string using binary (1024) units.
    /// </summary>
    /// <param name="size">Size in bytes.</param>
    /// <returns>
    /// A formatted string like "123 B", "456 KB", "789 MB" etc.
    /// </returns>
    public static string GetFormatedSize(long size)
    {
        if (size < 1024)
            return $"{size} B";
        
        string[] units = ["KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];
        long bytes = size / 1024;
        
        for (int i = 0; i < units.Length; i++)
        {
            if (i == units.Length - 1 || bytes < 1024)
                return $"{bytes} {units[i]}";
            
            bytes /= 1024;
        }
        return $"{bytes} RB";
    }
}
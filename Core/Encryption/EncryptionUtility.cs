using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Encryption;

/// <summary>
/// Provides utility methods for encrypting and decrypting text based on the operating system.
/// </summary>
public static class EncryptionUtility
{
    /// <summary>
    /// Logger instance for the EncryptionUtility class.
    /// </summary>
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(EncryptionUtility));

    /// <summary>
    /// Entropy value used for encryption on Windows.
    /// </summary>
    private static readonly byte[] s_entropy = "8AE1C425-241D-46A9-A8F8-6E8A68D2586C"u8.ToArray();

    /// <summary>
    /// Encryption key for Linux systems.
    /// </summary>
    private static readonly string linuxKey = "7CFD81CB-BD69-48DD-A84A-88A30A2574A9";

    /// <summary>
    /// Encryption key for macOS systems.
    /// </summary>
    private static readonly string macKey = "4E664B51-116D-416B-BA5D-93B7BD220187";

    /// <summary>
    /// Encrypts the given text based on the current operating system.
    /// </summary>
    /// <param name="text">The text to encrypt.</param>
    /// <returns>The encrypted text.</returns>
    public static string Encrypt(string text)
    {
        try
        {
            switch (OSHelper.GetOperatingSystem())
            {
                case EOperatingSystem.Windows:
                    return EncryptWin(text);
                case EOperatingSystem.Linux:
                    return EncryptLinux(text);
                case EOperatingSystem.MacOS:
                    return EncryptMac(text);
                default:
                    throw new Exception("Unknown operating system");
            }
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while encrypting text");
            _logger.Exc(ex);
            return text;
        }
    }

    /// <summary>
    /// Decrypts the given text based on the current operating system.
    /// </summary>
    /// <param name="text">The text to decrypt.</param>
    /// <returns>The decrypted text.</returns>
    public static string Decrypt(string text)
    {
        try
        {
            switch (OSHelper.GetOperatingSystem())
            {
                case EOperatingSystem.Windows:
                    return DecryptWin(text);
                case EOperatingSystem.Linux:
                    return DecryptLinux(text);
                case EOperatingSystem.MacOS:
                    return DecryptMac(text);
                default:
                    throw new Exception("Unknown operating system");
            }
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while decrypting text");
            _logger.Exc(ex);
            return text;
        }
    }

    /// <summary>
    /// Encrypts text using Windows-specific encryption.
    /// </summary>
    /// <param name="text">The text to encrypt.</param>
    /// <returns>The encrypted text.</returns>
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private static string EncryptWin(string text)
    {
        try
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(text);
            byte[] encryptedBytes = ProtectedData.Protect(plainBytes, s_entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while encrypting text on Windows");
            _logger.Exc(ex);
            return text;
        }
    }

    /// <summary>
    /// Decrypts text using Windows-specific decryption.
    /// </summary>
    /// <param name="text">The text to decrypt.</param>
    /// <returns>The decrypted text.</returns>
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private static string DecryptWin(string text)
    {
        try
        {
            byte[] cipherBytes = Convert.FromBase64String(text);
            byte[] decryptedBytes = ProtectedData.Unprotect(cipherBytes, s_entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while decrypting text on Windows");
            _logger.Exc(ex);
            return text;
        }
    }

    /// <summary>
    /// Encrypts text using Linux-specific encryption.
    /// </summary>
    /// <param name="text">The text to encrypt.</param>
    /// <returns>The encrypted text.</returns>
    private static string EncryptLinux(string text)
    {
        try
        {
            // TODO: Replace with better Linux encryption method
            // Currently using a simple AES encryption with a static key
            // This is not secure and should be replaced with a proper implementation
            // For now, I will use the Encrypt method with a predefined key
            // This is better than nothing, but not recommended for production use
            // After finishing other tasks, I will implement a proper Linux encryption method
            return Encrypt(linuxKey, text);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while encrypting text on Linux");
            _logger.Exc(ex);
            return text;
        }
    }

    /// <summary>
    /// Decrypts text using Linux-specific decryption.
    /// </summary>
    /// <param name="text">The text to decrypt.</param>
    /// <returns>The decrypted text.</returns>
    private static string DecryptLinux(string text)
    {
        try
        {
            // TODO: Replace with better Linux decryption method
            // Currently using a simple AES decryption with a static key
            // This is not secure and should be replaced with a proper implementation
            // For now, I will use the Decrypt method with a predefined key
            // This is better than nothing, but not recommended for production use
            // After finishing other tasks, I will implement a proper Linux decryption method
            return Decrypt(linuxKey, text);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while decrypting text on Linux");
            _logger.Exc(ex);
            return text;
        }
    }

    /// <summary>
    /// Encrypts text using macOS-specific encryption.
    /// </summary>
    /// <param name="text">The text to encrypt.</param>
    /// <returns>The encrypted text.</returns>
    private static string EncryptMac(string text)
    {
        try
        {
            // TODO: Replace with better macOS encryption method
            // Currently using a simple AES encryption with a static key
            // This is not secure and should be replaced with a proper implementation
            // For now, I will use the Encrypt method with a predefined key
            // This is better than nothing, but not recommended for production use
            // After finishing other tasks, I will implement a proper macOS encryption method
            return Encrypt(macKey, text);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while encrypting text on Mac");
            _logger.Exc(ex);
            return text;
        }
    }

    /// <summary>
    /// Decrypts text using macOS-specific decryption.
    /// </summary>
    /// <param name="text">The text to decrypt.</param>
    /// <returns>The decrypted text.</returns>
    private static string DecryptMac(string text)
    {
        try
        {
            // TODO: Replace with better macOS decryption method
            // Currently using a simple AES decryption with a static key
            // This is not secure and should be replaced with a proper implementation
            // For now, I will use the Decrypt method with a predefined key
            // This is better than nothing, but not recommended for production use
            // After finishing other tasks, I will implement a proper macOS decryption method
            return Decrypt(macKey, text);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while decrypting text on Mac");
            _logger.Exc(ex);
            return text;
        }
    }

    /// <summary>
    /// Encrypts text using AES encryption with the specified key.
    /// </summary>
    /// <param name="key">The encryption key.</param>
    /// <param name="plainText">The text to encrypt.</param>
    /// <returns>The encrypted text.</returns>
    public static string Encrypt(string key, string plainText)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    streamWriter.Write(plainText);

                array = memoryStream.ToArray();
            }
        }

        return Convert.ToBase64String(array);
    }

    /// <summary>
    /// Decrypts text using AES decryption with the specified key.
    /// </summary>
    /// <param name="key">The decryption key.</param>
    /// <param name="cipherText">The text to decrypt.</param>
    /// <returns>The decrypted text.</returns>
    public static string Decrypt(string key, string cipherText)
    {
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(cipherText);

        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = iv;
        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream memoryStream = new MemoryStream(buffer);
        using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using StreamReader streamReader = new StreamReader(cryptoStream);
        return streamReader.ReadToEnd();
    }
}
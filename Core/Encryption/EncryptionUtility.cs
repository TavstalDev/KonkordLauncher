using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
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
    private const string DP_PREFIX = "dpv1:";
    private const string WIN_PREFIX = "winv1:";
    private const string LINUX_PREFIX = "linuxv1:";
    private const string MAC_PREFIX   = "macv1:";


    /// <summary>
    /// A data protector instance used for encrypting and decrypting data.
    /// </summary>
    private static IDataProtector? _protector;

    /// <summary>
    /// Gets a value indicating whether the data protector is set.
    /// </summary>
    public static bool IsDataProtectorSet => _protector != null;
    
    /// <summary>
    /// Sets the data protection provider and initializes the data protector instance.
    /// </summary>
    /// <param name="provider">The data protection provider to create the protector from.</param>
    public static void SetDataProtectionProvider(IDataProtectionProvider provider)
    {
        // Prevent re-initialization if already set
        if (_protector != null)
            return;
        _protector = provider.CreateProtector("KonkordLauncher.Encryption.v1");
    }
    
    /// <summary>
    /// Entropy value used for encryption on Windows.
    /// </summary>
    private static readonly byte[] s_entropy = "8AE1C425-241D-46A9-A8F8-6E8A68D2586C"u8.ToArray();

    /// <summary>
    /// Encryption key for Linux systems.
    /// </summary>
    private static readonly string linuxKey = "898c3f038bfc090b62a32156aba62acda193e90e26baf0ab";

    /// <summary>
    /// Encryption key for macOS systems.
    /// </summary>
    private static readonly string macKey = "a83231f9eadeac1e8a3b875d86da77a43e911180d9e88968";

    /// <summary>
    /// Encrypts the given text based on the current operating system.
    /// </summary>
    /// <param name="text">The text to encrypt.</param>
    /// <returns>The encrypted text.</returns>
    public static string Encrypt(string text)
    {
        try
        {
            if (_protector != null)
                return DP_PREFIX + _protector.Protect(text);
            
            switch (OSHelper.GetOperatingSystem())
            {
                case EOperatingSystem.Windows:
                    return WIN_PREFIX + EncryptWin(text);
                case EOperatingSystem.Linux:
                    return LINUX_PREFIX + EncryptLinux(text);
                case EOperatingSystem.MacOS:
                    return MAC_PREFIX + EncryptMac(text);
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
    /// <param name="ignoreIfUnknown"></param>
    /// <returns>The decrypted text.</returns>
    public static string Decrypt(string text, bool ignoreIfUnknown = false)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.StartsWith(DP_PREFIX))
            {
                if (_protector == null)
                    throw new CryptographicException("Data protector is not set for DP encrypted text");
                return _protector!.Unprotect(text[DP_PREFIX.Length..]);
            }

            if (text.StartsWith(WIN_PREFIX))
                return DecryptWin(text[WIN_PREFIX.Length..]);

            if (text.StartsWith(LINUX_PREFIX))
                return DecryptLinux(text[LINUX_PREFIX.Length..]);
            
            if (text.StartsWith(MAC_PREFIX))
                return DecryptMac(text[MAC_PREFIX.Length..]);

            if (ignoreIfUnknown)
                return text;
            throw new CryptographicException("Unknown encryption format");
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while decrypting text");
            _logger.Exc(ex);
            return text;
        }
    }
    
    /// <summary>
    /// Reprotects an encrypted string by decrypting it and re-encrypting it using the current data protection provider.
    /// </summary>
    /// <param name="encrypted">The encrypted string to be reprotected.</param>
    /// <returns>
    /// The reprotected string if the data protection provider is set; otherwise, the original encrypted string.
    /// </returns>
    /// <remarks>
    /// This method handles different encryption formats based on their prefixes:
    /// - If the prefix is "dpv1:", the string is decrypted and re-encrypted using the current data protection provider.
    /// - If the prefix is "winv1:", "linuxv1:", or "macv1:", the string is decrypted using the respective platform-specific method
    ///   and then re-encrypted using the current data protection provider.
    /// - If the format is unknown, the string is re-encrypted as-is.
    /// If the data protection provider is not set, the method returns the original encrypted string.
    /// </remarks>
    public static string Reprotect(string encrypted)
    {
        if (string.IsNullOrEmpty(encrypted)) 
            return encrypted;

        try
        {
            if (_protector != null)
            {
                // if prefix is DP, decrypt & re-protect with current provider
                if (encrypted.StartsWith(DP_PREFIX))
                    return DP_PREFIX + _protector.Protect(_protector.Unprotect(encrypted[DP_PREFIX.Length..]));
                if (encrypted.StartsWith(WIN_PREFIX))
                {
                    // else if prefix is WIN, decrypt using Windows, then protect with DP
                    var plain = DecryptWin(encrypted[WIN_PREFIX.Length..]);
                    return DP_PREFIX + _protector.Protect(plain);
                }

                if (encrypted.StartsWith(LINUX_PREFIX))
                {
                    // else if prefix is LINUX, decrypt using Linux, then protect with DP
                    var plain = DecryptLinux(encrypted[LINUX_PREFIX.Length..]);
                    return DP_PREFIX + _protector.Protect(plain);
                }

                if (encrypted.StartsWith(MAC_PREFIX))
                {
                    // else if prefix is MAC, decrypt using Mac, then protect with DP
                    var plain = DecryptMac(encrypted[MAC_PREFIX.Length..]);
                    return DP_PREFIX + _protector.Protect(plain);
                }

                // Not known format, protect as-is
                return DP_PREFIX + _protector.Protect(encrypted);
            }

            return encrypted; // fallback: keep as-is
        }
        catch
        {
            return encrypted; // fail-safe
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
            if (string.IsNullOrEmpty(text))
                return text;
            
            // This is a fallback method for encryption
            // in case of IDataProtector is not set.
            // It is recommended to set IDataProtector for better security.
            // However, if not set, this method will be used.
            // This is not the most secure method, especially with a static key,
            // but it is better than nothing.
            // I could generate a custom key on each machine, but since I would need to store it
            // somewhere unencrypted, it would defeat the purpose of encryption.
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
            if (string.IsNullOrEmpty(text))
                return text;
            
            // This is a fallback method for encryption
            // in case of IDataProtector is not set.
            // It is recommended to set IDataProtector for better security.
            // However, if not set, this method will be used.
            // This is not the most secure method, especially with a static key,
            // but it is better than nothing.
            // I could generate a custom key on each machine, but since I would need to store it
            // somewhere unencrypted, it would defeat the purpose of encryption.
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
            if (string.IsNullOrEmpty(text))
                return text;
            
            // This is a fallback method for encryption
            // in case of IDataProtector is not set.
            // It is recommended to set IDataProtector for better security.
            // However, if not set, this method will be used.
            // This is not the most secure method, especially with a static key,
            // but it is better than nothing.
            // I could generate a custom key on each machine, but since I would need to store it
            // somewhere unencrypted, it would defeat the purpose of encryption.
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
            if (string.IsNullOrEmpty(text))
                return text;
            
            // This is a fallback method for encryption
            // in case of IDataProtector is not set.
            // It is recommended to set IDataProtector for better security.
            // However, if not set, this method will be used.
            // This is not the most secure method, especially with a static key,
            // but it is better than nothing.
            // I could generate a custom key on each machine, but since I would need to store it
            // somewhere unencrypted, it would defeat the purpose of encryption.
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
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.GenerateIV();

        using ICryptoTransform encryptor = aes.CreateEncryptor();

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipherText = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        
        byte[] combined = new byte[aes.IV.Length + cipherText.Length];
        Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherText, 0, combined, aes.IV.Length, cipherText.Length);

        return Convert.ToBase64String(combined);
    }

    /// <summary>
    /// Decrypts text using AES decryption with the specified key.
    /// </summary>
    /// <param name="key">The decryption key.</param>
    /// <param name="base64String">The text to decrypt.</param>
    /// <returns>The decrypted text.</returns>
    public static string Decrypt(string key, string base64String)
    {
        byte[] buffer = Convert.FromBase64String(base64String);
        byte[] iv = new byte[16];
        byte[] cipherText = new byte[buffer.Length - 16];
        Buffer.BlockCopy(buffer, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(buffer, iv.Length, cipherText, 0, cipherText.Length);
        
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = iv;
        using ICryptoTransform decryptor = aes.CreateDecryptor();
        byte[] plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
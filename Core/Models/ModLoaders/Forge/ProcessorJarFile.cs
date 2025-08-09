using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;

/// <summary>
/// Represents a processor for handling JAR files in the Forge mod loader.
/// Provides functionality to extract and parse the manifest file from a JAR archive.
/// </summary>
/// <remarks>
/// Source: https://github.com/CmlLib/CmlLib.Core.Installer.Forge
/// </remarks>
public class ProcessorJarFile
{
    /// <summary>
    /// Gets the path to the JAR file being processed.
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessorJarFile"/> class with the specified file path.
    /// </summary>
    /// <param name="path">The path to the JAR file.</param>
    public ProcessorJarFile(string path)
    {
        Path = path;
    }

    /// <summary>
    /// Extracts and parses the manifest file from the JAR archive.
    /// </summary>
    /// <returns>
    /// A dictionary containing key-value pairs from the manifest file, or <c>null</c> if the manifest is not found.
    /// </returns>
    public Dictionary<string, string?>? GetManifest()
    {
        string? text = null;
        using (FileStream baseInputStream = File.OpenRead(Path))
        {
            using ZipInputStream zipInputStream = new ZipInputStream(baseInputStream);
            while (zipInputStream.GetNextEntry() is { } nextEntry)
            {
                if (nextEntry.Name == "META-INF/MANIFEST.MF")
                {
                    text = ReadStreamString(zipInputStream);
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(text))
            return null;

        Dictionary<string, string?> dictionary = new Dictionary<string, string?>();
        string[] array = text.Split('\n');
        foreach (string text2 in array)
        {
            if (!string.IsNullOrWhiteSpace(text2))
            {
                string[] array2 = text2.Split(':');
                string key = array2[0].Trim();
                if (array2.Length == 1)
                {
                    dictionary.TryAdd(key, null);
                    continue;
                }

                if (array2.Length == 2)
                {
                    if (!dictionary.ContainsKey(key))
                        dictionary.Add(key, array2[1].Trim());
                    continue;
                }

                string value = string.Join(":", array2, 1, array2.Length - 1).Trim();
                dictionary.TryAdd(key, value);
            }
        }

        return dictionary;
    }

    /// <summary>
    /// Reads the content of a stream and converts it to a string using UTF-8 encoding.
    /// </summary>
    /// <param name="s">The stream to read from.</param>
    /// <returns>The string representation of the stream's content.</returns>
    private static string ReadStreamString(Stream s)
    {
        StringBuilder stringBuilder = new StringBuilder();
        byte[] array = new byte[1024];
        while (true)
        {
            int num = s.Read(array, 0, array.Length);
            if (num == 0)
                break;

            stringBuilder.Append(Encoding.UTF8.GetString(array, 0, num));
        }

        return stringBuilder.ToString();
    }
}
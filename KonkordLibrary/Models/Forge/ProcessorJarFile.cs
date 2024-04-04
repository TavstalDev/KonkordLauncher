using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

namespace KonkordLibrary.Models.Forge
{
    public class ProcessorJarFile
    {
        public string Path { get; private set; }

        public ProcessorJarFile(string path)
        {
            Path = path;
        }

        public Dictionary<string, string?>? GetManifest()
        {
            string text = null;
            using (FileStream baseInputStream = File.OpenRead(Path))
            {

                using ZipInputStream zipInputStream = new ZipInputStream(baseInputStream);
                ZipEntry nextEntry;
                while ((nextEntry = zipInputStream.GetNextEntry()) != null)
                {
                    if (nextEntry.Name == "META-INF/MANIFEST.MF")
                    {
                        text = readStreamString(zipInputStream);
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string[] array = text.Split('\n');
            foreach (string text2 in array)
            {
                if (!string.IsNullOrWhiteSpace(text2))
                {
                    string[] array2 = text2.Split(':');
                    string key = array2[0].Trim();
                    if (array2.Length == 1)
                    {
                        if (!dictionary.ContainsKey(key))
                            dictionary.Add(key, null);
                        continue;
                    }

                    if (array2.Length == 2)
                    {
                        if (!dictionary.ContainsKey(key))
                            dictionary.Add(key, array2[1].Trim());
                        continue;
                    }

                    string value = string.Join(":", array2, 1, array2.Length - 1).Trim();
                    if (!dictionary.ContainsKey(key))
                        dictionary.Add(key, value);
                }
            }

            return dictionary;
        }

        private static string readStreamString(Stream s)
        {
            StringBuilder stringBuilder = new StringBuilder();
            byte[] array = new byte[1024];
            while (true)
            {
                int num = s.Read(array, 0, array.Length);
                if (num == 0)
                {
                    break;
                }

                stringBuilder.Append(Encoding.UTF8.GetString(array, 0, num));
            }

            return stringBuilder.ToString();
        }
    }
}

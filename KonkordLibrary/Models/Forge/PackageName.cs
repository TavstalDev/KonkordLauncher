using System.IO;

namespace KonkordLibrary.Models.Forge
{
    public class PackageName
    {
        private readonly string[] names;

        public string this[int index] => names[index];

        public string Package => names[0];

        public string Name => names[1];

        public string Version => names[2];

        public static PackageName Parse(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            string[] array = name.Split(':');
            if (array.Length < 3)
            {
                throw new ArgumentException("invalid name");
            }

            return new PackageName(array);
        }

        private PackageName(string[] names)
        {
            this.names = names;
        }

        public string GetPath()
        {
            return GetPath("");
        }

        public string GetPath(string? nativeId)
        {
            return GetPath(nativeId, "jar");
        }

        public string GetPath(string? nativeId, string extension)
        {
            string text = string.Join("-", names, 1, names.Length - 1);
            if (!string.IsNullOrEmpty(nativeId))
            {
                text = text + "-" + nativeId;
            }

            text = text + "." + extension;
            return Path.Combine(GetDirectory(), text);
        }

        public string GetDirectory()
        {
            return Path.Combine(Package.Replace(".", "/"), Name, Version);
        }

        public string GetClassPath()
        {
            return Package + "." + Name;
        }
    }
}

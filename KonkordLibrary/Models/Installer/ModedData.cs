using KonkordLibrary.Models.Minecraft.Library;

namespace KonkordLibrary.Models.Installer
{
    public class ModedData
    {
        public string MainClass { get; set; }
        public VersionDetails VersionData { get; set; }
        public List<MCLibrary> Libraries { get; set; }

        public ModedData() { }

        public ModedData(string mainClass, VersionDetails versionData, List<MCLibrary> libraries)
        {
            MainClass = mainClass;
            VersionData = versionData;
            Libraries = libraries;
        }
    }
}

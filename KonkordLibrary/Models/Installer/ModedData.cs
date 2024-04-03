using KonkordLibrary.Models.GameManager;
using KonkordLibrary.Models.Minecraft.Library;

namespace KonkordLibrary.Models.Installer
{
    public class ModedData
    {
        public string MainClass { get; set; }
        public VersionResponse VersionData { get; set; }
        public List<MCLibrary> Libraries { get; set; }

        public ModedData() { }

        public ModedData(string mainClass, VersionResponse versionData, List<MCLibrary> libraries)
        {
            MainClass = mainClass;
            VersionData = versionData;
            Libraries = libraries;
        }
    }
}

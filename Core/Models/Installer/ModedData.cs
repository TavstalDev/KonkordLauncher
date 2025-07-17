using Tavstal.KonkordLauncher.Core.Models.Launcher;
using Tavstal.KonkordLauncher.Core.Models.Minecraft.Library;

namespace Tavstal.KonkordLauncher.Core.Models.Installer
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

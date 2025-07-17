using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Core.Models.Launcher
{
    public class VersionBase
    {
        public string Id { get; set; }
        public string VanillaId { get; set; }
        public EVersionType VersionType { get; set; }

        public VersionBase() { }

        public VersionBase(string id, string vanillaId, EVersionType versionType) { Id = id; VanillaId = vanillaId; VersionType = versionType; }
    }
}

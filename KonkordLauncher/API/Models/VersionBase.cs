using KonkordLauncher.API.Enums;

namespace KonkordLauncher.API.Models
{
    public class VersionBase
    {
        public string Id { get; set; }
        public EVersionType VersionType { get; set; }

        public VersionBase() { }

        public VersionBase(string id, EVersionType versionType) { Id = id; VersionType = versionType; }
    }
}

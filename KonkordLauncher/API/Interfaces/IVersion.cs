using KonkordLauncher.API.Enums;

namespace KonkordLauncher.API.Interfaces
{
    public interface IVersion
    {
        string Id { get; set; }
        EVersionType VersionType { get; set; }
    }
}

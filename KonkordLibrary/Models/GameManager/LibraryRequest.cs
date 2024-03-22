using KonkordLibrary.Enums;
using System.Windows.Controls;

namespace KonkordLibrary.Models.GameManager
{
    public class LibraryRequest
    {
        public EProfileKind Kind {  get; }
        public string VanillaVersion { get; }
        public string Version { get; }
        public string VersionJsonUrl { get; }
        public string VersionJsonPath { get; }
        public ProgressBar ProgressBar { get; }
        public Label Label { get; }

        public LibraryRequest() { }

        public LibraryRequest(EProfileKind kind, string vanillaVersion, string version, string versionJsonUrl, string versionJsonPath, ref ProgressBar progressBar, ref Label label)
        {
            Kind = kind;
            VanillaVersion = vanillaVersion;
            Version = version;
            VersionJsonUrl = versionJsonUrl;
            VersionJsonPath = versionJsonPath;
            ProgressBar = progressBar;
            Label = label;
        }

        public LibraryRequest(EProfileKind kind, string vanillaVersion, string version, string versionJsonUrl, string versionJsonPath, ProgressBar progressBar, Label label)
        {
            Kind = kind;
            VanillaVersion = vanillaVersion;
            Version = version;
            VersionJsonUrl = versionJsonUrl;
            VersionJsonPath = versionJsonPath;
            ProgressBar = progressBar;
            Label = label;
        }
    }
}

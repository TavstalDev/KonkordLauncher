
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.Java;

/// <summary>
/// Represents a Java mirror with download URLs for different architectures.
/// </summary>
public class JavaMirrorArchitecture
{
    /// <summary>
    /// Gets or sets the download URL for the x86_64 architecture.
    /// </summary>
    [JsonProperty("x86_64")]
    public string X86_64 { get; set; }

    /// <summary>
    /// Gets or sets the download URL for the ARM architecture.
    /// </summary>
    [JsonProperty("arm")]
    public string Arm { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaMirrorArchitecture"/> class with default values.
    /// </summary>
    public JavaMirrorArchitecture()
    {
        X86_64 = string.Empty;
        Arm = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaMirrorArchitecture"/> class with specified values.
    /// </summary>
    /// <param name="x8664">The download URL for the x86_64 architecture.</param>
    /// <param name="arm">The download URL for the ARM architecture.</param>
    public JavaMirrorArchitecture(string x8664, string arm)
    {
        X86_64 = x8664;
        Arm = arm;
    }
}
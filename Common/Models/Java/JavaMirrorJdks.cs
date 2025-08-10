using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.Java;

/// <summary>
/// Represents a collection of Java mirrors for different JDK versions and architectures.
/// </summary>
public class JavaMirrorJdks
{
    /// <summary>
    /// Gets or sets the Java mirror for JDK 7.
    /// </summary>
    [JsonProperty("java_7"), JsonPropertyName("java_7")]
    public JavaMirrorArchitecture Jdk7 { get; set; }

    /// <summary>
    /// Gets or sets the Java mirror for JDK 8.
    /// </summary>
    [JsonProperty("java_8"), JsonPropertyName("java_8")]
    public JavaMirrorArchitecture Jdk8 { get; set; }

    /// <summary>
    /// Gets or sets the Java mirror for JDK 17.
    /// </summary>
    [JsonProperty("java_17"), JsonPropertyName("java_17")]
    public JavaMirrorArchitecture Jdk17 { get; set; }

    /// <summary>
    /// Gets or sets the Java mirror for JDK 21.
    /// </summary>
    [JsonProperty("java_21"), JsonPropertyName("java_21")]
    public JavaMirrorArchitecture Jdk21 { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaMirrorJdks"/> class with default values.
    /// </summary>
    public JavaMirrorJdks()
    {
        Jdk7 = new JavaMirrorArchitecture();
        Jdk8 = new JavaMirrorArchitecture();
        Jdk17 = new JavaMirrorArchitecture();
        Jdk21 = new JavaMirrorArchitecture();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaMirrorJdks"/> class with specified values.
    /// </summary>
    /// <param name="jdk7">The Java mirror for JDK 7.</param>
    /// <param name="jdk8">The Java mirror for JDK 8.</param>
    /// <param name="jdk17">The Java mirror for JDK 17.</param>
    /// <param name="jdk21">The Java mirror for JDK 21.</param>
    public JavaMirrorJdks(JavaMirrorArchitecture jdk7, JavaMirrorArchitecture jdk8, JavaMirrorArchitecture jdk17, JavaMirrorArchitecture jdk21)
    {
        Jdk7 = jdk7;
        Jdk8 = jdk8;
        Jdk17 = jdk17;
        Jdk21 = jdk21;
    }
}
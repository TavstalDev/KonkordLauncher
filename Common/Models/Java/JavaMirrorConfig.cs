
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.Java;

/// <summary>
/// Represents the configuration for Java mirrors across different operating systems.
/// </summary>
public class JavaMirrorConfig
{
    /// <summary>
    /// Gets or sets the Java mirror configuration for Windows.
    /// </summary>
    [JsonProperty("windows")]
    public JavaMirrorJdks Windows { get; set; }
    
    /// <summary>
    /// Gets or sets the Java mirror configuration for Linux.
    /// </summary>
    [JsonProperty("linux")]
    public JavaMirrorJdks Linux { get; set; }
    
    /// <summary>
    /// Gets or sets the Java mirror configuration for macOS.
    /// </summary>
    [JsonProperty("mac")]
    public JavaMirrorJdks Mac { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="JavaMirrorConfig"/> class with default values.
    /// </summary>
    public JavaMirrorConfig()
    {
        Windows = new JavaMirrorJdks(
            // Java 7
            new JavaMirrorArchitecture(
                // x86_64
                "",
                // arm
                ""
            ),
            // Java 8
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin8-binaries/releases/download/jdk8u462-b08/OpenJDK8U-jdk_x64_windows_hotspot_8u462b08.zip",
                // arm
                ""
            ),
            // Java 16
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin16-binaries/releases/download/jdk-16.0.2%2B7/OpenJDK16U-jdk_x64_windows_hotspot_16.0.2_7.zip",
                // arm
                ""
            ),
            // Java 17
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin17-binaries/releases/download/jdk-17.0.9%2B9/OpenJDK17U-jdk_x86-32_windows_hotspot_17.0.9_9.zip",
                // arm
                ""
            ),
            // Java 21
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_x64_windows_hotspot_21.0.8_9.zip",
                // arm
                "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_aarch64_windows_hotspot_21.0.8_9.zip"
            ),
            // Java 25
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin25-binaries/releases/download/jdk-25.0.2%2B10/OpenJDK25U-jdk_x64_windows_hotspot_25.0.2_10.zip",
                // arm
                ""
                )
        );
        Linux = new JavaMirrorJdks(
            // Java 7
            new JavaMirrorArchitecture(
                // x86_64
                "",
                // arm
                ""
            ),
            // Java 8
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin8-binaries/releases/download/jdk8u462-b08/OpenJDK8U-jdk_x64_linux_hotspot_8u462b08.tar.gz",
                // arm
                "https://github.com/adoptium/temurin8-binaries/releases/download/jdk8u462-b08/OpenJDK8U-jdk_aarch64_linux_hotspot_8u462b08.tar.gz"
            ),
            // Java 16
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin16-binaries/releases/download/jdk-16.0.2%2B7/OpenJDK16U-jdk_x64_linux_hotspot_16.0.2_7.tar.gz",
                // arm
                "https://github.com/adoptium/temurin16-binaries/releases/download/jdk-16.0.2%2B7/OpenJDK16U-jdk_aarch64_linux_hotspot_16.0.2_7.tar.gz"
            ),
            // Java 17
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin17-binaries/releases/download/jdk-17.0.9%2B9/OpenJDK17U-jdk_x64_linux_hotspot_17.0.9_9.tar.gz",
                // arm
                "https://github.com/adoptium/temurin17-binaries/releases/download/jdk-17.0.9%2B9/OpenJDK17U-jdk_aarch64_linux_hotspot_17.0.9_9.tar.gz"
            ),
            // Java 21
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_x64_linux_hotspot_21.0.8_9.tar.gz",
                // arm
                "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_aarch64_linux_hotspot_21.0.8_9.tar.gz"
            ),
            // Java 25
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin25-binaries/releases/download/jdk-25.0.2%2B10/OpenJDK25U-jdk_x64_linux_hotspot_25.0.2_10.tar.gz",
                // arm
                "https://github.com/adoptium/temurin25-binaries/releases/download/jdk-25.0.2%2B10/OpenJDK25U-jdk_aarch64_linux_hotspot_25.0.2_10.tar.gz"
            )
        );
        Mac = new JavaMirrorJdks(
            // Java 7
            new JavaMirrorArchitecture(
                // x86_64
                "",
                // arm
                ""
            ),
            // Java 8
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin8-binaries/releases/download/jdk8u462-b08/OpenJDK8U-jdk_x64_mac_hotspot_8u462b08.tar.gz",
                // arm
                ""
            ),
            // Java 16
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin16-binaries/releases/download/jdk-16.0.2%2B7/OpenJDK16U-jdk_x64_mac_hotspot_16.0.2_7.tar.gz",
                // arm
                ""
            ),
            // Java 17
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin17-binaries/releases/download/jdk-17.0.9%2B9/OpenJDK17U-jdk_x64_mac_hotspot_17.0.9_9.tar.gz",
                // arm
                "https://github.com/adoptium/temurin17-binaries/releases/download/jdk-17.0.9%2B9/OpenJDK17U-jdk_aarch64_mac_hotspot_17.0.9_9.tar.gz"
            ),
            // Java 21
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_x64_mac_hotspot_21.0.8_9.tar.gz",
                // arm
                "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_aarch64_mac_hotspot_21.0.8_9.tar.gz"
            ),
            // Java 25
            new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin25-binaries/releases/download/jdk-25.0.2%2B10/OpenJDK25U-jdk_x64_mac_hotspot_25.0.2_10.tar.gz",
                // arm
                "https://github.com/adoptium/temurin25-binaries/releases/download/jdk-25.0.2%2B10/OpenJDK25U-jdk_aarch64_mac_hotspot_25.0.2_10.tar.gz"
            )
        );
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="JavaMirrorConfig"/> class with specified values.
    /// </summary>
    /// <param name="windows">The Java mirror configuration for Windows.</param>
    /// <param name="linux">The Java mirror configuration for Linux.</param>
    /// <param name="mac">The Java mirror configuration for macOS.</param>
    public JavaMirrorConfig(JavaMirrorJdks windows, JavaMirrorJdks linux, JavaMirrorJdks mac)
    {
        Windows = windows;
        Linux = linux;
        Mac = mac;
    }
}
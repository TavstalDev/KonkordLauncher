using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs.InstanceConfig;

/// <summary>
///     Holds other settings of an instance, such as custom libraries and server options.
/// </summary>
public class InstanceMiscConfig
{
    /// <summary>
    ///     Creates a new instance with default values.
    /// </summary>
    public InstanceMiscConfig()
    {
        UseCustomGlfw = false;
        CustomGlfwPath = string.Empty;
        UseCustomOpenAL = false;
        CustomOpenALPath = string.Empty;
        OverrideAccount = false;
        AccountId = string.Empty;
        JoinServerOnLaunch = false;
        ServerAddress = string.Empty;
    }

    /// <summary>
    ///     Creates a new instance with the given values.
    /// </summary>
    /// <param name="useCustomGlfw">Whether a custom GLFW library is used.</param>
    /// <param name="customGlfwPath">The path to the custom GLFW library.</param>
    /// <param name="useCustomOpenAl">Whether a custom OpenAL library is used.</param>
    /// <param name="customOpenAlPath">The path to the custom OpenAL library.</param>
    /// <param name="overrideAccount">Whether another account is used.</param>
    /// <param name="accountId">The ID of the account to use.</param>
    /// <param name="joinServerOnLaunch">Whether the game joins a server on launch.</param>
    /// <param name="serverAddress">The address of the server to join.</param>
    public InstanceMiscConfig(bool useCustomGlfw, string customGlfwPath, bool useCustomOpenAl, string customOpenAlPath,
        bool overrideAccount, string accountId, bool joinServerOnLaunch, string serverAddress)
    {
        UseCustomGlfw = useCustomGlfw;
        CustomGlfwPath = customGlfwPath;
        UseCustomOpenAL = useCustomOpenAl;
        CustomOpenALPath = customOpenAlPath;
        OverrideAccount = overrideAccount;
        AccountId = accountId;
        JoinServerOnLaunch = joinServerOnLaunch;
        ServerAddress = serverAddress;
    }

    /// <summary>
    ///     Gets or sets a value that indicates whether a custom GLFW library is used.
    /// </summary>
    [JsonPropertyName("useCustomGlfw")]
    public bool UseCustomGlfw { get; set; }

    /// <summary>
    ///     Gets or sets the path to the custom GLFW library.
    /// </summary>
    [JsonPropertyName("customGlfwPath")]
    public string CustomGlfwPath { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether a custom OpenAL library is used.
    /// </summary>
    [JsonPropertyName("useCustomOpenAL")]
    public bool UseCustomOpenAL { get; set; }

    /// <summary>
    ///     Gets or sets the path to the custom OpenAL library.
    /// </summary>
    [JsonPropertyName("customOpenALPath")]
    public string CustomOpenALPath { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether another account is used.
    /// </summary>
    [JsonPropertyName("overrideAccount")]
    public bool OverrideAccount { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the account to use.
    /// </summary>
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the game joins a server on launch.
    /// </summary>
    [JsonPropertyName("joinServerOnLaunch")]
    public bool JoinServerOnLaunch { get; set; }

    /// <summary>
    ///     Gets or sets the address of the server to join.
    /// </summary>
    [JsonPropertyName("serverAddress")]
    public string ServerAddress { get; set; }
}

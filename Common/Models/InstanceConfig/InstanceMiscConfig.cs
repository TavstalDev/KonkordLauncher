
using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Common.Models.InstanceConfig;

/// <summary>
/// Represents miscellaneous configuration settings for a game instance, including custom libraries,
/// account overrides, and server connection options.
/// </summary>
public class InstanceMiscConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether a custom GLFW library should be used.
    /// </summary>
    [JsonPropertyName("useCustomGlfw")]
    public bool UseCustomGlfw { get; set; }

    /// <summary>
    /// Gets or sets the file system path to the custom GLFW library.
    /// </summary>
    [JsonPropertyName("customGlfwPath")]
    public string CustomGlfwPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a custom OpenAL library should be used.
    /// </summary>
    [JsonPropertyName("useCustomOpenAL")]
    public bool UseCustomOpenAL { get; set; }

    /// <summary>
    /// Gets or sets the file system path to the custom OpenAL library.
    /// </summary>
    [JsonPropertyName("customOpenALPath")]
    public string CustomOpenALPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the account should be overridden with a specific account ID.
    /// </summary>
    [JsonPropertyName("overrideAccount")]
    public bool OverrideAccount { get; set; }

    /// <summary>
    /// Gets or sets the account ID to use when overriding the account.
    /// </summary>
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the game should automatically join a server on launch.
    /// </summary>
    [JsonPropertyName("joinServerOnLaunch")]
    public bool JoinServerOnLaunch { get; set; }

    /// <summary>
    /// Gets or sets the address of the server to join on launch.
    /// </summary>
    [JsonPropertyName("serverAddress")]
    public string ServerAddress { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceMiscConfig"/> class with default values.
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
    /// Initializes a new instance of the <see cref="InstanceMiscConfig"/> class with specified values.
    /// </summary>
    /// <param name="useCustomGlfw">Whether a custom GLFW library should be used.</param>
    /// <param name="customGlfwPath">The file system path to the custom GLFW library.</param>
    /// <param name="useCustomOpenAl">Whether a custom OpenAL library should be used.</param>
    /// <param name="customOpenAlPath">The file system path to the custom OpenAL library.</param>
    /// <param name="overrideAccount">Whether the account should be overridden with a specific account ID.</param>
    /// <param name="accountId">The account ID to use when overriding the account.</param>
    /// <param name="joinServerOnLaunch">Whether the game should automatically join a server on launch.</param>
    /// <param name="serverAddress">The address of the server to join on launch.</param>
    public InstanceMiscConfig(bool useCustomGlfw, string customGlfwPath, bool useCustomOpenAl, string customOpenAlPath, bool overrideAccount, string accountId, bool joinServerOnLaunch, string serverAddress)
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
}
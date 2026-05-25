using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Instance;

/// <summary>
/// Represents the miscellaneous configuration model for a Minecraft instance, including custom libraries, account overrides, and server settings.
/// </summary>
public partial class InstanceMiscConfigModel : ObservableObject
{
    /// <summary>
    /// Gets or sets a value indicating whether a custom GLFW library should be used.
    /// </summary>
    [ObservableProperty]
    public partial bool UseCustomGlfw { get; set; }

    /// <summary>
    /// Gets or sets the file path to the custom GLFW library.
    /// </summary>
    [ObservableProperty]
    public partial string CustomGlfwPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a custom OpenAL library should be used.
    /// </summary>
    [ObservableProperty]
    public partial bool UseCustomOpenAL { get; set; }

    /// <summary>
    /// Gets or sets the file path to the custom OpenAL library.
    /// </summary>
    [ObservableProperty]
    public partial string CustomOpenALPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the account should be overridden.
    /// </summary>
    [ObservableProperty]
    public partial bool OverrideAccount { get; set; }

    /// <summary>
    /// Gets or sets the account ID to use when overriding the account.
    /// </summary>
    [ObservableProperty]
    public partial string AccountId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the instance should join a server on launch.
    /// </summary>
    [ObservableProperty]
    public partial bool JoinServerOnLaunch { get; set; }

    /// <summary>
    /// Gets or sets the address of the server to join on launch.
    /// </summary>
    [ObservableProperty]
    public partial string ServerAddress { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceMiscConfigModel"/> class with default values.
    /// </summary>
    public InstanceMiscConfigModel()
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
    /// Initializes a new instance of the <see cref="InstanceMiscConfigModel"/> class with specified values.
    /// </summary>
    /// <param name="useCustomGlfw">Whether a custom GLFW library should be used.</param>
    /// <param name="customGlfwPath">The file path to the custom GLFW library.</param>
    /// <param name="useCustomOpenAl">Whether a custom OpenAL library should be used.</param>
    /// <param name="customOpenAlPath">The file path to the custom OpenAL library.</param>
    /// <param name="overrideAccount">Whether the account should be overridden.</param>
    /// <param name="accountId">The account ID to use when overriding the account.</param>
    /// <param name="joinServerOnLaunch">Whether the instance should join a server on launch.</param>
    /// <param name="serverAddress">The address of the server to join on launch.</param>
    public InstanceMiscConfigModel(bool useCustomGlfw, string customGlfwPath, bool useCustomOpenAl, string customOpenAlPath, bool overrideAccount, string accountId, bool joinServerOnLaunch, string serverAddress)
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
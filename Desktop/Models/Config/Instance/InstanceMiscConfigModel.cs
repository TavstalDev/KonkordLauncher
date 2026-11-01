using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Instance;

/// <summary>
///     Holds other settings for an instance, such as custom libraries and server options.
/// </summary>
public partial class InstanceMiscConfigModel : ObservableObject
{
    /// <summary>
    ///     Creates a new instance with default settings.
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
    ///     Creates a new instance with the given settings.
    /// </summary>
    /// <param name="useCustomGlfw">Whether a custom GLFW library is used.</param>
    /// <param name="customGlfwPath">The path to the custom GLFW library.</param>
    /// <param name="useCustomOpenAl">Whether a custom OpenAL library is used.</param>
    /// <param name="customOpenAlPath">The path to the custom OpenAL library.</param>
    /// <param name="overrideAccount">Whether another account is used.</param>
    /// <param name="accountId">The ID of the account to use.</param>
    /// <param name="joinServerOnLaunch">Whether the instance joins a server on launch.</param>
    /// <param name="serverAddress">The address of the server to join.</param>
    public InstanceMiscConfigModel(bool useCustomGlfw, string customGlfwPath, bool useCustomOpenAl,
        string customOpenAlPath, bool overrideAccount, string accountId, bool joinServerOnLaunch, string serverAddress)
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
    [ObservableProperty]
    public partial bool UseCustomGlfw { get; set; }

    /// <summary>
    ///     Gets or sets the path to the custom GLFW library.
    /// </summary>
    [ObservableProperty]
    public partial string CustomGlfwPath { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether a custom OpenAL library is used.
    /// </summary>
    [ObservableProperty]
    public partial bool UseCustomOpenAL { get; set; }

    /// <summary>
    ///     Gets or sets the path to the custom OpenAL library.
    /// </summary>
    [ObservableProperty]
    public partial string CustomOpenALPath { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether another account is used.
    /// </summary>
    [ObservableProperty]
    public partial bool OverrideAccount { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the account to use.
    /// </summary>
    [ObservableProperty]
    public partial string AccountId { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the instance joins a server on launch.
    /// </summary>
    [ObservableProperty]
    public partial bool JoinServerOnLaunch { get; set; }

    /// <summary>
    ///     Gets or sets the address of the server to join.
    /// </summary>
    [ObservableProperty]
    public partial string ServerAddress { get; set; }
}

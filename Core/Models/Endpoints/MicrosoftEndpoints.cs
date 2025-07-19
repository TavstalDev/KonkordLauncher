namespace Tavstal.KonkordLauncher.Core.Models.Endpoints;

/// <summary>
/// Provides endpoint URLs and helper methods for Microsoft, Xbox, and Minecraft authentication.
/// </summary>
public static class MicrosoftEndpoints
{
    /// <summary>
    /// Generates the Microsoft authentication URL with the specified client ID and redirect URL.
    /// </summary>
    /// <param name="clientId">The client ID of the application.</param>
    /// <param name="redirectUrl">The redirect URL to be used after authentication.</param>
    /// <returns>A formatted Microsoft authentication URL.</returns>
    public static string MakeMicrosoftAuthUrl(string clientId, string redirectUrl)
    {
        return $"https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize?" +
               $"client_id={clientId}" +
               $"&response_type=code" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUrl)}" +
               $"&response_mode=query" +
               $"&scope=XboxLive.signin%20offline_access";
    }

    /// <summary>
    /// The URL for obtaining Microsoft OAuth tokens.
    /// </summary>
    public const string MicrosoftTokenUrl = $"https://login.microsoftonline.com/consumers/oauth2/v2.0/token";

    /// <summary>
    /// The URL for Xbox user authentication.
    /// </summary>
    public const string XboxAuthUrl = $"https://user.auth.xboxlive.com/user/authenticate";

    /// <summary>
    /// The URL for obtaining Xbox XSTS tokens.
    /// </summary>
    public const string XboxXstsUrl = $"https://xsts.auth.xboxlive.com/xsts/authorize";

    /// <summary>
    /// The URL for authenticating with Minecraft using an Xbox token.
    /// </summary>
    public const string MinecraftAuthUrl = "https://api.minecraftservices.com/authentication/login_with_xbox";

    /// <summary>
    /// The URL for retrieving the Minecraft user profile.
    /// </summary>
    public const string MinecraftProfileUrl = "https://api.minecraftservices.com/minecraft/profile";

    /// <summary>
    /// The URL for checking Minecraft ownership and entitlements.
    /// </summary>
    public const string MinecraftOwnershipUrl = "https://api.minecraftservices.com/entitlements/mcstore";
    
    /// <summary>
    /// The URL for retrieving the Minecraft version manifest.
    /// </summary>
    public const string MinecraftManifestUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
    
    /// <summary>
    /// The base URL for downloading Minecraft resources.
    /// </summary>
    public const string MinecraftResourcesUrl = "https://resources.download.minecraft.net";
}
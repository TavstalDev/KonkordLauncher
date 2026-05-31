using System.Net;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Accounts;
using Tavstal.KonkordLauncher.Core.Models.Microsoft;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

namespace Tavstal.KonkordLauncher.Core.Services.Abstractions.Auth;

/// <summary>
/// Provides an abstraction for Microsoft authentication services including OAuth2 device code flow,
/// token exchange, and Minecraft profile retrieval.
/// </summary>
public interface IMicrosoftAuthService
{
    /// <summary>
    /// Gets the current authentication status.
    /// </summary>
    EAuthStatus AuthStatus { get; }
    
    /// <summary>
    /// Gets the currently loaded Mojang profile associated with the authenticated account.
    /// </summary>
    MojangProfile? MojangProfile { get; }
    
    /// <summary>
    /// Gets the authenticated account information, if available.
    /// </summary>
    Account? Account { get; }
    
    /// <summary>
    /// Delegate used to notify subscribers when the Microsoft authentication status changes.
    /// </summary>
    /// <param name="status">The new authentication status.</param>
    public delegate void AuthStatusChangedHandler(EAuthStatus status);

    /// <summary>
    /// Occurs when the Microsoft authentication status changes.
    /// </summary>
    event AuthStatusChangedHandler? OnAuthStatusChanged;
    
    /// <summary>
    /// Sets the Microsoft OAuth2 client ID for authentication requests.
    /// </summary>
    /// <param name="clientId">The client ID from the Microsoft Azure application registration.</param>
    void SetClientId(string clientId);
    
    /// <summary>
    /// Resets the authentication state, clearing all account and profile information.
    /// </summary>
    void Reset();

    /// <summary>
    /// Opens the Microsoft authentication URL in the default web browser.
    /// </summary>
    void OpenAuthenticationUrl();

    /// <summary>
    /// Handles an incoming HTTP request from Microsoft's OAuth2 authorization callback.
    /// </summary>
    /// <param name="request">The HTTP request containing the authorization code or error information.</param>
    /// <param name="progressReporter">Optional reporter for tracking authentication progress.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleHttpRequestAsync(HttpListenerRequest request, IProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a device code for the Microsoft OAuth2 device code flow.
    /// </summary>
    /// <param name="progressReporter">Optional reporter for tracking the creation progress.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>A <see cref="DeviceCodeResult"/> containing the device code and user code,
    /// or null if the operation fails.</returns>
    Task<DeviceCodeResult?> CreateDeviceCodeAsync(IProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the status of a device code authentication and completes the flow if authenticated.
    /// </summary>
    /// <param name="deviceCode">The device code to check for authentication status.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CheckDeviceCodeAsync(string deviceCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges a Microsoft access token for an Xbox Live token.
    /// </summary>
    /// <param name="token">The Microsoft access token from OAuth2 authentication.</param>
    /// <param name="refreshToken">The Microsoft refresh token for future token renewals.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task XboxTokenCallAsync(string token, string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges an Xbox Live token for an Xbox XSTS (Xbox Security Token Service) token.
    /// </summary>
    /// <param name="token">The Xbox Live authentication token.</param>
    /// <param name="refreshToken">The Microsoft refresh token for future token renewals.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task XboxXstsCallAsync(string token, string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges Xbox XSTS token for a Minecraft access token.
    /// </summary>
    /// <param name="token">The Xbox XSTS token.</param>
    /// <param name="refreshToken">The Microsoft refresh token for future token renewals.</param>
    /// <param name="userHash">The user hash from the Xbox XSTS authentication response.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MinecraftAccessCallAsync(string token, string refreshToken, string userHash,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies that the authenticated user owns a Minecraft game license.
    /// </summary>
    /// <param name="mcToken">The Minecraft access token.</param>
    /// <param name="refreshToken">The Microsoft refresh token for future token renewals.</param>
    /// <param name="expireSeconds">The expiration time of the access token in seconds.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CheckMinecraftOwnershipAsync(string mcToken, string refreshToken, int expireSeconds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the authenticated user's Minecraft profile information.
    /// </summary>
    /// <param name="mcToken">The Minecraft access token.</param>
    /// <param name="refreshToken">The Microsoft refresh token for future token renewals.</param>
    /// <param name="expireSecs">The expiration time of the access token in seconds.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task GetMinecraftProfileAsync(string mcToken, string refreshToken, int expireSecs,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an expired Microsoft access token using a refresh token.
    /// </summary>
    /// <param name="token">The Microsoft refresh token obtained from a previous authentication.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>A task representing the asynchronous operation that returns true if refresh was successful, false otherwise.</returns>
    Task<bool> RefreshLoginAsync(string token, CancellationToken cancellationToken = default);
}
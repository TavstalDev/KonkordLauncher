
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.Microsoft;

/// <summary>
/// Represents the result of a device code request for Microsoft OAuth authentication.
/// Contains information such as the device code, user code, verification URI, expiration time, and polling interval.
/// </summary>
public class DeviceCodeResult
{
    /// <summary>
    /// Gets or sets the device code used for authentication.
    /// </summary>
    [JsonProperty("device_code")]
    public string DeviceCode { get; set; }

    /// <summary>
    /// Gets or sets the user code displayed to the user for authentication.
    /// </summary>
    [JsonProperty("user_code")]
    public string UserCode { get; set; }

    /// <summary>
    /// Gets or sets the URI where the user can verify their device code.
    /// </summary>
    [JsonProperty("verification_uri")]
    public string VerificationUri { get; set; }

    /// <summary>
    /// Gets or sets the time in seconds until the device code expires.
    /// </summary>
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the interval in seconds at which the client should poll for token updates.
    /// </summary>
    [JsonProperty("interval")]
    public int Interval { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceCodeResult"/> class.
    /// </summary>
    public DeviceCodeResult() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceCodeResult"/> class with the specified properties.
    /// </summary>
    /// <param name="deviceCode">The device code used for authentication.</param>
    /// <param name="userCode">The user code displayed to the user for authentication.</param>
    /// <param name="verificationUri">The URI where the user can verify their device code.</param>
    /// <param name="expiresIn">The time in seconds until the device code expires.</param>
    /// <param name="interval">The interval in seconds at which the client should poll for token updates.</param>
    public DeviceCodeResult(string deviceCode, string userCode, string verificationUri, int expiresIn, int interval)
    {
        DeviceCode = deviceCode;
        UserCode = userCode;
        VerificationUri = verificationUri;
        ExpiresIn = expiresIn;
        Interval = interval;
    }
}
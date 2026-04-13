using System.Text;

namespace Tavstal.KonkordLauncher.Core.Helpers.Domain;

public static class GameHelper
{

    /// <summary>
    /// Retrieves the UUID (Universally Unique Identifier) of a player based on the provided username.
    /// </summary>
    /// <param name="username">The username of the player.</param>
    /// <returns>
    /// A <see cref="string"/> representing the UUID of the player.
    /// </returns>
    private static string GetPlayerUUID(string username)
    {
        //new GameProfile(UUID.nameUUIDFromBytes(("OfflinePlayer:" + name).getBytes(Charsets.UTF_8)), name));
        byte[] rawresult = System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(username));
        //set the version to 3 -> Name based md5 hash
        rawresult[6] = (byte)(rawresult[6] & 0x0f | 0x30);
        //IETF variant
        rawresult[8] = (byte)(rawresult[8] & 0x3f | 0x80);
        //convert to string and remove any - if any
        string finalresult = BitConverter.ToString(rawresult).Replace("-", "");
        //formatting
        finalresult = finalresult.Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
        return finalresult;
    }

    /// <summary>
    /// Retrieves the UUID (Universally Unique Identifier) of an offline player based on the provided username.
    /// </summary>
    /// <param name="username">The username of the offline player.</param>
    /// <returns>
    /// A <see cref="string"/> representing the UUID of the offline player.
    /// </returns>
    public static string GetOfflinePlayerUUID(string username)
    {
        return GetPlayerUUID($"OfflinePlayer:{username}");
    }
}
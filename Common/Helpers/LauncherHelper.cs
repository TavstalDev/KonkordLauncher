using ICSharpCode.SharpZipLib.Core;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Helpers;

namespace Tavstal.KonkordLauncher.Common.Helpers;

public static class LauncherHelper
{
    public static LauncherSettings? GetLauncherSettings()
    {
        // TODO: Handle the case where the file does not exist or is not readable.
        return JsonConvert.DeserializeObject<LauncherSettings>(File.ReadAllText(PathHelper.LauncherConfigPath));
    }
}
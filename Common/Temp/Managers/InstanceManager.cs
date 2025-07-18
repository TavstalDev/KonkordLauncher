using System.Diagnostics;
using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Instances;
using Tavstal.KonkordLauncher.Core.Models.Instances.CurseForge;
using Tavstal.KonkordLauncher.Core.Models.Launcher;

namespace Tavstal.KonkordLauncher.Core.Managers
{
    public static class InstanceManager
    {
        private static IProgressReporter _progressReporter;
        // TODO: Should be moved to Desktop

        public static async Task HandleInstanceZipImport(string filePath, IProgressReporter progressReporter)
        {
            _progressReporter = progressReporter;

            // Get the temporal dir name from file
            filePath = filePath.Replace("/", "\\");
            string zipName = filePath.Remove(filePath.LastIndexOf('.'), filePath.Length - filePath.LastIndexOf('.')); // remove file format
            zipName = zipName.Remove(0, zipName.LastIndexOf('\\') + 1);

            string instanceDir = Path.Combine(IOHelper.TempDir, zipName);
            
            if (Directory.Exists(instanceDir))
                IOHelper.DeleteDirectory(instanceDir);

            // Extract the zip
            ZipFile.ExtractToDirectory(filePath, instanceDir);

            try
            {
                // Handle the extracted stuff
                if (File.Exists(Path.Combine(instanceDir, "manifest.json")))
                {
                    await HandleCurseForgeZipImport(instanceDir);
                }
                else if (File.Exists(Path.Combine(instanceDir, "instance.json")))
                {
                    await HandleKonkordZipImport(instanceDir);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            // Delete temporal files
            if (Directory.Exists(instanceDir))
                IOHelper.DeleteDirectory(instanceDir);
        }

        /// <summary>
        /// Asynchronously handles the import of a CurseForge modpack extracted ZIP file located at the specified directory.
        /// </summary>
        /// <param name="instanceDir">The directory where the CurseForge modpack extracted ZIP file is located.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        private static async Task HandleCurseForgeZipImport(string instanceDir)
        {
            string manifestPath = Path.Combine(instanceDir, "manifest.json");
            string rawManifestJson = await File.ReadAllTextAsync(manifestPath);
            CurseForgeInstance? manifest = JsonConvert.DeserializeObject<CurseForgeInstance>(rawManifestJson);
            if (manifest == null)
                return;

            LauncherSettings? launcherSettings = await IOHelper.GetLauncherSettingsAsync();
            if (launcherSettings == null)
                return;

            // Import the Profile
            string key = Guid.NewGuid().ToString();
            Profile profile = manifest.GetProfile();
            launcherSettings.Profiles.Add(key, profile);
            launcherSettings.SelectedProfile = key;
            await JsonHelper.WriteJsonFileAsync(IOHelper.LauncherJsonFile, launcherSettings);

            // Import overrides like texture packs, options etc...
            string overridesDir = Path.Combine(instanceDir, manifest.Overrides);
            if (!Directory.Exists(overridesDir))
                return;

            // Create Profile Dir
            if (!Directory.Exists(profile.GameDirectory))
                Directory.CreateDirectory(profile.GameDirectory);

            // Import files
            UpdateProgressbarTranslated(0, "ui_copying_instance_ovverrides");
            foreach (string file in Directory.GetFiles(overridesDir))
            {
                string? fileName = Path.GetFileName(file)?.ToString();
                if (string.IsNullOrEmpty(fileName))
                    continue;

                string targetFilePath = Path.Combine(profile.GameDirectory, fileName);
                if (File.Exists(file) && !File.Exists(targetFilePath))
                {
                    File.Copy(file, targetFilePath);
                }
            }

            // Import dir
            foreach (string dirPath in Directory.GetDirectories(overridesDir))
            {
                string? dirName = Path.GetDirectoryName(dirPath)?.ToString();
                if (string.IsNullOrEmpty(dirName))
                    continue;
                string targetDirPath = Path.Combine(profile.GameDirectory, dirName);
                if (Directory.Exists(dirPath))
                {
                    IOHelper.MoveDirectory(dirPath, targetDirPath, true, false, false);
                }
            }

            // Create mods dir
            string modsDir = Path.Combine(profile.GameDirectory, "mods");
            if (!Directory.Exists(modsDir))
                Directory.CreateDirectory(modsDir);

            // Download Mods
            UpdateProgressbarTranslated(0, "ui_downloading_mods");
            foreach (CurseFile mod in manifest.Files)
            {
                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (sender, e) =>
                {
                    UpdateProgressbarTranslated(e, "ui_downloading_mod_manifest", new object[] { mod.ProjectId, e.ToString("0.00") });
                };

                string? rawManifest = await HttpHelper.GetStringAsync($"https://www.curseforge.com/api/v1/mods/{mod.ProjectId}/files/{mod.FileId}", progress);
                if (rawManifest == null)
                    continue;

                JObject modManifest = JObject.Parse(rawManifest);

                string? modName = modManifest?["data"]?["fileName"]?.ToString();
                string? modDisplayName = modManifest?["data"]?["displayName"]?.ToString() ?? modName;
                if (string.IsNullOrEmpty(modName))
                    continue;

                JToken? gameVersions = modManifest?["data"]?["gameVersions"];

                string parentDir = modsDir;
                // Not a mod check
                if (gameVersions != null && modName.EndsWith(".zip"))
                {
                    // Shader
                    if (gameVersions.Any(x => x.ToString() == "OptiFine") || gameVersions.Any(x => x.ToString() == "Iris") || gameVersions.Any(x => x.ToString() == "Oculus"))
                    {
                        parentDir = Path.Combine(profile.GameDirectory, "shaderpacks");
                    }
                    else // Resourcepack
                    {
                        parentDir = Path.Combine(profile.GameDirectory, "resourcepacks");
                    }
                }

               

                string filePath = Path.Combine(parentDir, modName);
                if (!mod.Required)
                    filePath += ".disabled";
                if (File.Exists(filePath))
                    continue;

                progress = new Progress<double>();
                progress.ProgressChanged += (sender, e) =>
                {
                    UpdateProgressbarTranslated(e, "ui_downloading_mod", new object[] { modDisplayName, e.ToString("0.00") });
                };

                byte[]? bytes = await HttpHelper.GetByteArrayAsync($"https://www.curseforge.com/api/v1/mods/{mod.ProjectId}/files/{mod.FileId}/download", progress);
                if (bytes == null)
                    continue;

                if (!Directory.Exists(parentDir))
                    Directory.CreateDirectory(parentDir);

                await File.WriteAllBytesAsync(filePath, bytes);
            }
        }

        /// <summary>
        /// Asynchronously handles the import of a Konkord modpack extracted ZIP file located at the specified directory.
        /// </summary>
        /// <param name="instanceDir">The directory where the Konkord modpack extracted ZIP file is located.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        private static async Task HandleKonkordZipImport(string instanceDir)
        {
            string profileJsonPath = Path.Combine(instanceDir, "profile.json");
            string rawManifest = await File.ReadAllTextAsync(profileJsonPath);
            InstanceManifest? manifest = JsonConvert.DeserializeObject<InstanceManifest>(rawManifest);
            if (manifest == null)
                return;

            LauncherSettings? launcherSettings = await IOHelper.GetLauncherSettingsAsync();
            if (launcherSettings == null)
                return;

            // Import the Profile
            string key = Guid.NewGuid().ToString();
            launcherSettings.Profiles.Add(key, manifest.GetProfile());
            launcherSettings.SelectedProfile = key;
            await JsonHelper.WriteJsonFileAsync(IOHelper.LauncherJsonFile, launcherSettings);

            // Import overrides like texture packs, options etc...
            string overridesDir = Path.Combine(instanceDir, "overrides");
            if (!Directory.Exists(overridesDir))
                return;

            // Create game directory
            VersionDetails versionDetails = GameHelper.GetProfileVersionDetails(manifest.Kind, manifest.VersionId, manifest.VersionVanillaId, manifest.GameDirectory);
            if (!Directory.Exists(versionDetails.GameDir))
                Directory.CreateDirectory(versionDetails.GameDir);

            Progress<double> progress = new Progress<double>();

            // Import files
            UpdateProgressbarTranslated(0, "ui_copying_instance_ovverrides");
            foreach (string file in Directory.GetFiles(instanceDir))
            {
                string fileName = file.Replace("/", "\\");
                fileName = fileName.Remove(0, fileName.LastIndexOf("\\") + 1);

                string filePath = Path.Combine(versionDetails.GameDir, fileName);
                if (File.Exists(file) && !File.Exists(filePath))
                    File.Copy(file, filePath);
            }

            // Import directories
            foreach (string dirPath in Directory.GetDirectories(overridesDir))
            {
                string? dirName = Path.GetDirectoryName(dirPath)?.ToString();
                if (string.IsNullOrEmpty(dirName))
                    continue;
                string targetDirPath = Path.Combine(versionDetails.GameDir, dirName);
                if (Directory.Exists(dirPath))
                {
                    IOHelper.MoveDirectory(dirPath, targetDirPath, true, false, false);
                }
            }

            string modDir = Path.Combine(versionDetails.GameDir, "mods");
            if (!Directory.Exists(modDir))
                Directory.CreateDirectory(modDir);

            // Download mods
            foreach (string mod in manifest.ModList)
            {
                string modName = mod.Remove(0, mod.LastIndexOf('/') + 1);
                string modUrl = manifest.FileServer != null ? Path.Combine(manifest.FileServer, mod) : mod;

                progress = new Progress<double>();
                progress.ProgressChanged += (sender, e) =>
                {
                    UpdateProgressbarTranslated(e, "ui_downloading_mod", new object[] { modName, e.ToString("0.00") });
                };

                byte[]? modBytes = await HttpHelper.GetByteArrayAsync(modUrl, progress);
                if (modBytes == null)
                    continue;

                string modPath = Path.Combine(modDir, modName);
                if (!File.Exists(modPath))
                    await File.WriteAllBytesAsync(modPath, modBytes);
            }
        }

        public static async Task ExportKonkordInstance(Profile profile, string targetPath)
        {
            // Create tempDir
            string tempDir = Path.Combine(IOHelper.TempDir, profile.Name);
            if (Directory.Exists(tempDir))
            {
                IOHelper.DeleteDirectory(tempDir);
                Directory.CreateDirectory(tempDir);
            }
            else
                Directory.CreateDirectory(tempDir);

            VersionDetails versionDetails = GameHelper.GetProfileVersionDetails(profile.Kind, profile.VersionId, profile.VersionVanillaId, profile.GameDirectory);

            // Copy directories
            foreach (string dir in Directory.GetDirectories(versionDetails.GameDir))
            {
                if (!(dir.Contains("config") || dir.Contains("logs") || dir.Contains("downloads") || dir.Contains(".fabric") || !dir.Contains("mods") || dir.Contains("resourcepacks") || dir.Contains("saves") || dir.Contains("shaderpacks")))
                    continue;

                string localDir = dir.Replace("/", "\\");
                string dirName = localDir.Remove(0, localDir.LastIndexOf('\\') + 1);

                IOHelper.MoveDirectory(dir, Path.Combine(tempDir, "overrides", dirName), true, false);
            }

            // Copy files
            foreach (string file in Directory.GetFiles(versionDetails.GameDir))
            {
                string localFile = file.Replace("/", "\\");
                string fileName = localFile.Remove(0, localFile.LastIndexOf('\\') + 1);

                File.Copy(file, Path.Combine(tempDir, "overrides", fileName), true);
            }

            // Write manifest
            InstanceManifest manifest = new InstanceManifest(string.Empty, new List<string>(), 1, profile.Name, profile.Icon, profile.VersionId, profile.VersionVanillaId, profile.Type, profile.Kind, profile.Resolution, profile.GameDirectory, profile.JavaPath, profile.JVMArgs, profile.Memory, profile.LauncherVisibility);
            string manifestJson = Path.Combine(tempDir, "profile.json");
            await File.WriteAllTextAsync(manifestJson, JsonConvert.SerializeObject(manifest));


            // Copy zip
            ZipFile.CreateFromDirectory(tempDir, targetPath);
        }

        public static async Task ExportCurseForgeInstance(Profile profile, string targetPath)
        {
            await Task.Delay(1);
        }

        /// <summary>
        /// Updates the progress bar with the specified percentage and text.
        /// </summary>
        /// <param name="percent">The percentage value for the progress bar.</param>
        /// <param name="text">The text to display along with the progress bar.</param>
        private static void UpdateProgressbarTranslated(double percent, string text, params object[]? args)
        {
            if (_label == null || _progressBar == null)
                return;

            _label.Content = TranslationManager.Translate(text, args);
            _progressBar.Value = percent > _progressBar.Maximum ? _progressBar.Maximum : percent;
        }
    }
}

using KonkordLauncher.API.Helpers;
using KonkordLauncher.API.Models;
using KonkordLauncher.API.Models.Minecraft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KonkordLauncher
{
    /// <summary>
    /// Interaction logic for LaunchWindow.xaml
    /// </summary>
    public partial class LaunchWindow : Window
    {
        public Profile SelectedProfile { get; set; }

        public LaunchWindow()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
            // Gets the primary screen parameters.
            int nWidth = (int)SystemParameters.PrimaryScreenWidth;
            int nHeight = (int)SystemParameters.PrimaryScreenHeight;

            double oldHeight = Height;
            double oldWidth = Width;
            Height = nHeight * 0.55; // 0.55 means 55% of the screen's height
            Width = nWidth * 0.55; // 0.55 means 55% of the screen's width

            // Get the multiplier how much should the window's content to be resized.
            double heightMultiplier = Height / oldHeight;
            double widthMultiplier = Width / oldWidth;

            Resize(bo_title_row, heightMultiplier, widthMultiplier);
            Resize(img_window_icon, heightMultiplier, widthMultiplier);
            ResizeFont(l_WindowName, heightMultiplier, widthMultiplier);
            ResizeFont(bt_window_close, heightMultiplier, widthMultiplier);
            ResizeFont(bt_window_minimize, heightMultiplier, widthMultiplier);
            ResizeFont(bt_window_normal, heightMultiplier, widthMultiplier);
            ResizeFont(bt_window_maximize, heightMultiplier, widthMultiplier);

            Resize(bo_leftmenu, heightMultiplier, widthMultiplier);
            Resize(gr_account, heightMultiplier, widthMultiplier);
            Resize(img_account, heightMultiplier, widthMultiplier);
            ResizeFont(la_account_name, heightMultiplier, widthMultiplier);
            ResizeFont(la_account_type, heightMultiplier, widthMultiplier);
            ResizeFont(btn_launch_logout, heightMultiplier, widthMultiplier);

            Resize(bo_launcher_settings, heightMultiplier, widthMultiplier);
            ResizeFont(lab_launcher_settings_icon, heightMultiplier, widthMultiplier);
            ResizeFont(lab_launcher_settings, heightMultiplier, widthMultiplier);
            ResizeFont(btn_launcher_settings, heightMultiplier, widthMultiplier);

            Resize(bo_launcher_new, heightMultiplier, widthMultiplier);
            ResizeFont(btn_launcher_new, heightMultiplier, widthMultiplier);

            Resize(listbox, heightMultiplier, widthMultiplier);


            Resize(bo_launch_play, heightMultiplier, widthMultiplier);
            ResizeFont(btn_launch_play, heightMultiplier, widthMultiplier);
            ResizeFont(lab_launc_play, heightMultiplier, widthMultiplier);
            ResizeFont(lab_selected_profile, heightMultiplier, widthMultiplier);

            Resize(bo_install, heightMultiplier, widthMultiplier);
            Resize(bo_install_status, heightMultiplier, widthMultiplier);
            ResizeFont(lab_install, heightMultiplier, widthMultiplier);

            RefreshInstances();

            /*DataContext = Enumerable.Range(1, 10)
                                .Select(x => new Profile()
                                {
                                    Name = "Profile" + x.ToString(),
                                    Icon = ProfileIcon.Icons[x].Path
                                });*/
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshAccount();
        }

        #region Functions
        /// <summary>
        /// Resizes a FrameworkElement based on the provided height and width multipliers.
        /// </summary>
        /// <param name="element">The FrameworkElement to resize.</param>
        /// <param name="heightMulti">The multiplier for the height.</param>
        /// <param name="widthMulti">The multiplier for the width.</param>
        private void Resize(FrameworkElement element, double heightMulti, double widthMulti)
        {
            element.Height = element.Height * heightMulti;
            element.Width = element.Width * widthMulti;
            element.Margin = new Thickness
            {
                Bottom = element.Margin.Bottom * heightMulti,
                Left = element.Margin.Left * widthMulti,
                Right = element.Margin.Right * widthMulti,
                Top = element.Margin.Top * heightMulti,
            };
        }

        /// <summary>
        /// Resizes the font of a Control based on the provided height and width multipliers.
        /// </summary>
        /// <param name="element">The Control whose font to resize.</param>
        /// <param name="heightMulti">The multiplier for the height.</param>
        /// <param name="widthMulti">The multiplier for the width.</param>
        private void ResizeFont(Control element, double heightMulti, double widthMulti)
        {
            element.Height = element.Height * heightMulti;
            element.Width = element.Width * widthMulti;
            element.Margin = new Thickness
            {
                Bottom = element.Margin.Bottom * heightMulti,
                Left = element.Margin.Left * widthMulti,
                Right = element.Margin.Right * widthMulti,
                Top = element.Margin.Top * heightMulti,
            };
            element.FontSize = element.FontSize * widthMulti;
        }

        private async Task RefreshAccount()
        {
            AccountData? accountData = await JsonHelper.ReadJsonFile<AccountData>(System.IO.Path.Combine(IOHelper.MainDirectory, "accounts.json"));
            if (accountData == null)
                return;

            var account = accountData.Accounts[accountData.SelectedAccountId];
            if (account == null)
                return;

            la_account_name.Content = account.DisplayName;
            switch (account.Type)
            {
                case API.Enums.EAccountType.OFFLINE:
                    {
                        la_account_type.Content = "Offline account";
                        break;
                    }
                case API.Enums.EAccountType.MICROSOFT:
                    {
                        la_account_type.Content = "Microsoft account";
                        break;
                    }
            }

            string avatarPath = System.IO.Path.Combine(IOHelper.CacheDir, $"{account.DisplayName}.png");
            if (!File.Exists(avatarPath))
            {
                using (var client = new HttpClient())
                {
                    BitmapImage bi;
                    byte[] array = await client.GetByteArrayAsync($"https://mineskin.eu/head/{account.DisplayName}/256.png");
                    await File.WriteAllBytesAsync(avatarPath, array);
                    using (var ms = new MemoryStream(array))
                    {
                        bi = new BitmapImage();
                        bi.BeginInit();
                        bi.CreateOptions = BitmapCreateOptions.None;
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.StreamSource = ms;
                        bi.EndInit();
                    }

                    img_account.Source = bi;
                }
            }
            else
            {
                BitmapImage bi;
                byte[] array = await File.ReadAllBytesAsync(avatarPath);
                using (var ms = new MemoryStream(array))
                {
                    bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CreateOptions = BitmapCreateOptions.None;
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = ms;
                    bi.EndInit();
                }

                img_account.Source = bi;
            }
        }

        private void RefreshInstances()
        {
            LauncherSettings? settings = IOHelper.GetLauncherSettings();
            if (settings == null)
                return;

            if (settings.Profiles == null)
            {
                DataContext = null;
                return;
            }

            var profiles = settings.Profiles;
            DataContext = profiles.Values.ToList();

  
            if (profiles.TryGetValue(settings.SelectedProfile, out Profile selectedProfile))
            {
                listbox.SelectedItem = selectedProfile;
                lab_selected_profile.Content = selectedProfile.Name.ToLower();
            }
            else
            {
                listbox.SelectedIndex = 0;
                selectedProfile = settings.Profiles.Values.ToList().ElementAt(0);
                lab_selected_profile.Content = selectedProfile.Name.ToLower();
            }
        }
        #endregion

        #region Window Events
        /// <summary>
        /// Event handler for closing the window when a specific button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void WindowClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Event handler for minimizing the window when a specific button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void WindowMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Event handler for restoring the window when a specific button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void WindowRestore_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            bt_window_maximize.IsEnabled = true;
            bt_window_normal.IsEnabled = false;
            bt_window_normal.Visibility = Visibility.Hidden;
            bt_window_maximize.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Event handler for maximizing the window when a specific button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void WindowMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            bt_window_maximize.IsEnabled = false;
            bt_window_normal.IsEnabled = true;
            bt_window_normal.Visibility = Visibility.Visible;
            bt_window_maximize.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Settings Button
        private void LauncherSettings_Click(object sender, RoutedEventArgs e)
        {
            bo_launcher_settings.Background = new SolidColorBrush(Color.FromScRgb(0.25f, 0, 0, 0));
            bo_launcher_settings.BorderBrush = new SolidColorBrush(Color.FromScRgb(0.25f, 0, 0, 0));
        }

        private void LauncherSettings_MouseEnter(object sender, MouseEventArgs e)
        {
            bo_launcher_settings.Background = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
            bo_launcher_settings.BorderBrush = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
        }

        private void LauncherSettings_MouseLeave(object sender, MouseEventArgs e)
        {
            bo_launcher_settings.Background = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
            bo_launcher_settings.BorderBrush = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
        }
        #endregion

        #region Launcher New Instance
        private void LauncherNew_MouseEnter(object sender, MouseEventArgs e)
        {
            bo_launcher_new.Background = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
            bo_launcher_new.BorderBrush = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
        }

        private void LauncherNew_MouseLeave(object sender, MouseEventArgs e)
        {
            bo_launcher_new.Background = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
            bo_launcher_new.BorderBrush = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
        }

        #endregion

        private async void LaunchLogout_Click(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(IOHelper.MainDirectory, "accounts.json");
            AccountData? accountData = await JsonHelper.ReadJsonFile<AccountData>(path);
            if (accountData == null)
                return;

            accountData.SelectedAccountId = string.Empty;
            await JsonHelper.WriteJsonFile(path, accountData);
            AuthWindow window = new AuthWindow();
            window.Show();
            this.Close();
        }

        private readonly double _launchMaxStep = 4;
        private async void LaunchPlay_Click(object sender, RoutedEventArgs e)
        {
            btn_launch_play.IsEnabled = false;

            AccountData? accountData = await  JsonHelper.ReadJsonFile<AccountData>(System.IO.Path.Combine(IOHelper.MainDirectory, "accounts.json"));
            if (accountData == null)
            {
                NotificationHelper.SendError("Could not launch the game because failed to get the account details.", "Error");
                return;
            }

            Account account = accountData.Accounts[accountData.SelectedAccountId];
            if (account == null)
            {
                NotificationHelper.SendError("Could not launch the game because failed to get the current account.", "Error");
                return;
            }

            LauncherSettings? settings = IOHelper.GetLauncherSettings();
            if (settings == null)
            {
                NotificationHelper.SendError("Could not launch the game because failed to get the launcher settings.", "Error");
                return;
            }

            Profile? selectedProfile = settings.Profiles[settings.SelectedProfile];
            if (selectedProfile == null)
            {
                NotificationHelper.SendError("Could not launch the game because an invalid profile is selected.", "Error");
                return;
            }

            string version = string.Empty;
            string versionDirectory = string.Empty;
            string versionJsonPath = string.Empty;
            string versionJarPath = string.Empty;
            string gameDir = string.Empty;
            string libraryBundle = string.Empty;
            string assetIndex = string.Empty;
            //System.IO.Path.Combine(IOHelper.VersionsDir, )

            bo_install.IsEnabled = true;
            bo_install.Visibility = Visibility.Visible;
            pb_status.Value = 0d;
            lab_install.Content = $"Reading the manifest file...";
            List<string> argumnets = new List<string>();

            switch (selectedProfile.Kind)
            {
                case API.Enums.EProfileKind.VANILLA:
                    {
                        // Read the Version Manifest
                        VersionManifest? manifest = await JsonHelper.ReadJsonFile<VersionManifest>(System.IO.Path.Combine(IOHelper.MainfestDir, "vanillaManifest.json"));
                        if (manifest == null)
                        {
                            NotificationHelper.SendError("Failed to get the vanilla manifest file.", "Error");
                            return;
                        }

                        pb_status.Value = 1d / _launchMaxStep * 100;
                        lab_install.Content = $"Checking the version directory and file...";

                        // Check the profile type
                        switch (selectedProfile.Type)
                        {
                            case API.Enums.EProfileType.LATEST_RELEASE:
                                {
                                    version = manifest.Latest.Release;
                                    versionDirectory = System.IO.Path.Combine(IOHelper.VersionsDir, manifest.Latest.Release);
                                    versionJsonPath = System.IO.Path.Combine(versionDirectory, $"{manifest.Latest.Release}.json");
                                    versionJarPath = System.IO.Path.Combine(versionDirectory, $"{manifest.Latest.Release}.jar");
                                    gameDir = System.IO.Path.Combine(IOHelper.InstancesDir, $"{manifest.Latest.Release}");
                                    break;
                                }
                            case API.Enums.EProfileType.LATEST_SNAPSHOT:
                                {
                                    version = manifest.Latest.Snapshot;
                                    versionDirectory = System.IO.Path.Combine(IOHelper.VersionsDir, manifest.Latest.Snapshot);
                                    versionJsonPath = System.IO.Path.Combine(versionDirectory, $"{manifest.Latest.Snapshot}.json");
                                    versionJarPath = System.IO.Path.Combine(versionDirectory, $"{manifest.Latest.Snapshot}.jar");
                                    gameDir = System.IO.Path.Combine(IOHelper.InstancesDir, $"{manifest.Latest.Snapshot}");
                                    break;
                                }
                            case API.Enums.EProfileType.CUSTOM:
                                {
                                    version = selectedProfile.VersionId;
                                    versionDirectory = System.IO.Path.Combine(IOHelper.VersionsDir, selectedProfile.VersionId);
                                    versionJsonPath = System.IO.Path.Combine(versionDirectory, $"{selectedProfile.VersionId}.json");
                                    versionJarPath = System.IO.Path.Combine(versionDirectory, $"{selectedProfile.VersionId}.jar");
                                    gameDir = selectedProfile.GameDirectory;
                                    break;
                                }
                            case API.Enums.EProfileType.KONKORD_CREATE:
                            case API.Enums.EProfileType.KONKORD_VANILLAPLUS:
                                {
                                    // MODED, DO NOTHING
                                    break;
                                }
                        }

                        if (!Directory.Exists(gameDir))
                            Directory.CreateDirectory(gameDir);

                        if (!Directory.Exists(versionDirectory))
                            Directory.CreateDirectory(versionDirectory);

                        // Find the right version
                        MCVersion? mcVersion = manifest.Versions.Find(x => x.Id == version);
                        if (mcVersion == null)
                        {
                            NotificationHelper.SendError("Failed to get the minecraft version from the manifest file.", "Error");
                            return;
                        }

                        // Check the version json file
                        if (!File.Exists(versionJsonPath))
                        {
                            pb_status.Value = 2d / _launchMaxStep * 100;
                            lab_install.Content = $"Downloading the version json file...";
                            using (var client = new HttpClient())
                            {
                                byte[] result = await client.GetByteArrayAsync(mcVersion.Url);
                                await File.WriteAllBytesAsync(versionJsonPath, result);
                            }
                        }

                        // Get the version json object
                        JObject obj = JObject.Parse(await File.ReadAllTextAsync(versionJsonPath));
                        // Get theasset index
                        assetIndex = obj["assetIndex"]["id"].ToString();

                        // Download the assets 
                        lab_install.Content = $"Downloading assets... 0%";
                        int totalAssetSize = int.Parse(obj["assetIndex"]["totalSize"].ToString());
                        string assetIndexDir = Path.Combine(IOHelper.AssetsDir, "indexes");
                        string assetIndexJsonPath = Path.Combine(assetIndexDir, $"{assetIndex}.json");
                        if (!File.Exists(assetIndexJsonPath))
                        {
                            using (var client = new HttpClient())
                            {
                                byte[] array = await client.GetByteArrayAsync(obj["assetIndex"]["url"].ToString());
                                await File.WriteAllBytesAsync(assetIndexJsonPath, array);
                            }
                        }

                        // Check Objects
                        string assetObjectDir = Path.Combine(IOHelper.AssetsDir, "objects");
                        if (!Directory.Exists(assetObjectDir))
                            Directory.CreateDirectory(assetObjectDir);

                        string rawAssetJson = await File.ReadAllTextAsync(assetIndexJsonPath);
                        JToken assetIndexToken = JObject.Parse(rawAssetJson)["objects"];
                        using (var client = new HttpClient())
                        {
                            string hash = string.Empty;
                            string objectDir = string.Empty;
                            string objectPath = string.Empty;
                            int downloadedAssetSize = 0;
                            foreach (JToken token in assetIndexToken.ToList())
                            {
                                hash = token.First["hash"].ToString();
                                objectDir = Path.Combine(assetObjectDir, hash.Substring(0, 2));
                                objectPath = Path.Combine(objectDir, $"{hash}");

                                if (!Directory.Exists(objectDir))
                                    Directory.CreateDirectory(objectDir);

                                if (!File.Exists(objectPath))
                                {

                                    byte[] array = await client.GetByteArrayAsync($"https://resources.download.minecraft.net/{hash.Substring(0, 2)}/{hash}");
                                    await File.WriteAllBytesAsync(objectPath, array);

                                }
                                downloadedAssetSize += int.Parse(token.First["size"].ToString());
                                pb_status.Value = (double)downloadedAssetSize / (double)totalAssetSize * 100d;
                                lab_install.Content = $"Downloading assets... {pb_status.Value.ToString("0.00")}%";
                            }
                        }

                        // Check the version jar file
                        if (!File.Exists(versionJarPath))
                        {
                            pb_status.Value = 2d / _launchMaxStep * 100;
                            lab_install.Content = $"Downloading the version jar file...";
                            string jarDownloadUrl = obj["downloads"]["client"]["url"].ToString();
                            using (var client = new HttpClient())
                            {
                                byte[] result = await client.GetByteArrayAsync(jarDownloadUrl);
                                await File.WriteAllBytesAsync(versionJarPath, result);
                            }
                        }
                        pb_status.Value = 3d / _launchMaxStep * 100;
                        lab_install.Content = $"Downloading libraries...";
                        // Get the libraries 
                        JArray jArray = JArray.Parse(obj["libraries"].ToString(Formatting.None));
                        // Check the libraries
                        using (var client = new HttpClient())
                        {
                            foreach (var jToken in jArray)
                            {
                                lab_install.Content = $"Downloading the {jToken["name"]} library...";
                                // Get the path
                                string libPath = Path.Combine(IOHelper.LibrariesDir, jToken["downloads"]["artifact"]["path"].ToString());
                                libraryBundle += $"{libPath};";
                                // Get the directory
                                string libDirectory = libPath.Remove(libPath.LastIndexOf('/'), libPath.Length - (libPath.LastIndexOf('/')));
                                if (!File.Exists(libPath))
                                {
                                    string libUrl = jToken["downloads"]["artifact"]["url"].ToString();
                                    byte[] result = await client.GetByteArrayAsync(libUrl);
                                    if (!Directory.Exists(libDirectory))
                                        Directory.CreateDirectory(libDirectory);
                                    await File.WriteAllBytesAsync(libPath, result);
                                }
                            }
                        }

                        libraryBundle += $"{versionJarPath}";
                        pb_status.Value = 4d / _launchMaxStep * 100;
                        lab_install.Content = $"Launching the game...";
                        break;
                    }
                case API.Enums.EProfileKind.FORGE:
                    {
                        // TODO
                        break;
                    }
                case API.Enums.EProfileKind.FABRIC:
                    {
                        // TODO
                        break;
                    }
                case API.Enums.EProfileKind.QUILT:
                    {
                        // TODO
                        break;
                    }
            }

            #region Arguments
            // The JVM args set by the user
            argumnets.Add($"{selectedProfile.JVMArgs}");
            // Minimum Memory
            // If someone brokes it by setting the maximum memory lower than 256MB... do not hit them
            argumnets.Add($"-Xms256M");
            // Maximum Memory
            if (selectedProfile.Memory > 0 && selectedProfile.Memory <= 256)
                argumnets.Add($"-Xmx{selectedProfile.Memory}G");
            else if (selectedProfile.Memory > 256)
                argumnets.Add($"-Xmx{selectedProfile.Memory}M");
            else
                argumnets.Add($"-Xmx4G");
            // Sets the main java class (?)
            argumnets.Add($"-DMcEmu=net.minecraft.client.main.Main");
            // Mojang's tricky solution for intel drivers
            argumnets.Add($"-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump");
            // Set the launcher name
            argumnets.Add($"-Dminecraft.launcher.brand=konkord-launcher");
            // Set the launcher version
            argumnets.Add($"-Dminecraft.launcher.version=1.0.0");
            // Ignore Invalid Certificates
            argumnets.Add($"-Dfml.ignoreInvalidMinecraftCertificates=true");
            // Ingone Patch Discrepancies
            argumnets.Add($"-Dfml.ignorePatchDiscrepancies=true");
            // Include the depedencies
            argumnets.Add($"-cp \"{libraryBundle}\"");

            // Minecraft Args
            // The main class
            argumnets.Add("net.minecraft.client.main.Main");
            argumnets.Add($"--username {account.DisplayName}");
            argumnets.Add($"--version {version}");
            argumnets.Add($"--gameDir {selectedProfile.GameDirectory}");
            // Directory of the assets
            argumnets.Add($"--assetsDir {IOHelper.AssetsDir}");
            // Asset Index
            argumnets.Add($"--assetIndex {assetIndex}");
            argumnets.Add($"--uuid {account.UUID}");
            argumnets.Add($"--accessToken {account.AccessToken}");
            argumnets.Add($"--clientId 0"); // todo
            argumnets.Add($"--xuid 0"); // todo
            argumnets.Add($"--userType msa");
            argumnets.Add($"--versionType release");
            // Screen resolution
            if (selectedProfile.Resolution != null)
            {
                if (selectedProfile.Resolution.IsFullScreen)
                {
                    argumnets.Add($"--width {(int)SystemParameters.PrimaryScreenWidth}");
                    argumnets.Add($"--height {(int)SystemParameters.PrimaryScreenHeight}");
                }
                else
                {
                    if (selectedProfile.Resolution.X > 0)
                        argumnets.Add($"--width {selectedProfile.Resolution.X}");
                    else
                        argumnets.Add($"--width {(int)SystemParameters.PrimaryScreenWidth / 2}");

                    if (selectedProfile.Resolution.Y > 0)
                        argumnets.Add($"--height {selectedProfile.Resolution.Y}");
                    else
                        argumnets.Add($"--height {(int)SystemParameters.PrimaryScreenHeight / 2}");
                }
            }
            else
            {
                argumnets.Add($"--width {(int)SystemParameters.PrimaryScreenWidth / 2}");
                argumnets.Add($"--height {(int)SystemParameters.PrimaryScreenHeight / 2}");
            }
            #endregion

            btn_launch_play.IsEnabled = true;
            bo_install.IsEnabled = false;
            bo_install.Visibility = Visibility.Hidden;
            //Launch game instance
            var psi = new ProcessStartInfo()
            {
                FileName = $"{selectedProfile.JavaPath}javaw",
                Arguments = string.Join(' ', argumnets),
                UseShellExecute = true,
                // Debug stuff, don't forget to disable ShellExecute while debugging:
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
            };

            Process? p = Process.Start(psi);

            switch (selectedProfile.LauncherVisibility)
            {
                case API.Enums.ELaucnherVisibility.HIDE_AND_REOPEN_ON_GAME_CLOSE:
                    {
                        this.Hide();
                        await p.WaitForExitAsync();
                        this.Show();
                        break;
                    }
                case API.Enums.ELaucnherVisibility.CLOSE_ON_GAME_START:
                    {
                        this.Close();
                        break;
                    }
                case API.Enums.ELaucnherVisibility.KEEP_OPEN:
                    {
                        // Just do nothing
                        break;
                    }
            }

            // Debug stuff:
            //p.ErrorDataReceived += (sender, args) => NotificationHelper.SendError(args.Data, "E");
            //p.BeginErrorReadLine();
            //p.BeginOutputReadLine();
            //await p.WaitForExitAsync();
        }

        private void listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Profile? selectedProfile = listbox.SelectedItem as Profile;
            if (selectedProfile != null)
                lab_selected_profile.Content = selectedProfile.Name.ToLower();
        }
    }
}

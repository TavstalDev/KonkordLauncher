using KonkordLauncher.API.Helpers;
using KonkordLauncher.API.Models;
using KonkordLauncher.API.Models.Minecraft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using KonkordLauncher.API.Interfaces;

namespace KonkordLauncher
{
    /// <summary>
    /// Interaction logic for LaunchWindow.xaml
    /// </summary>
    public partial class LaunchWindow : Window
    {
        public Profile SelectedProfile { get; set; }
        private readonly double _heightMultiplier;
        private readonly double _widthMultiplier;
        // CHANGE THESE IF THE LISTBOX TEMPLATE SIZES CHANGE
        #region Listbox Fields
        public double ListLabelFontSize { get; set; } = 10;
        public double ListLabelHeight { get; set; } = 19;
        public double ListLabelWidth { get; set; } = 131;
        public Thickness ListLabelMargin { get; set; } = new Thickness(5,2,0,2);
        public double ListBorderHeight { get; set; } = 24;
        public double ListBorderWidth { get; set; } = 24;
        public Thickness ListBorderMargin { get; set; } = new Thickness(5,2,0,2);
        #endregion

        public LaunchWindow() : this(null)
        {

        }

        public LaunchWindow(string? versionId)
        {
            InitializeComponent();
            Loaded += Window_Loaded;
            // Gets the primary screen parameters.
            int nWidth = (int)SystemParameters.PrimaryScreenWidth;
            int nHeight = (int)SystemParameters.PrimaryScreenHeight;

            double oldHeight = Height;
            double oldWidth = Width;
            Height = nHeight * 0.6; // 0.55 means 55% of the screen's height
            Width = nWidth * 0.6; // 0.55 means 55% of the screen's width

            // Get the multiplier how much should the window's content to be resized.
            _heightMultiplier = Height / oldHeight;
            _widthMultiplier = Width / oldWidth;

            #region Resize Elements
            #region Main Window
            WindowHelper.Resize(bo_title_row, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(img_window_icon, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(l_WindowName, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(bt_window_close, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(bt_window_minimize, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(bt_window_normal, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(bt_window_maximize, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(bo_leftmenu, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(gr_account, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(img_account, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(la_account_name, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(la_account_type, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(bo_language, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_language, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_language_icon, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_language, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(bo_new_instance, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_new_instance, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_new_instance_icon, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_new_instance, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(listbox_launchinstances, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(bo_launch_play, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_launch_play, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_launc_play, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_selected_profile, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(bo_install, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(bo_install_status, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_install, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(ref listbox_launchinstances, ListBorderHeight, ListBorderWidth, ListBorderMargin, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(ref listbox_launchinstances, ListLabelFontSize, ListLabelHeight, ListLabelWidth, ListLabelMargin, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(bo_instances, _heightMultiplier, _widthMultiplier);
            #endregion
            #region Instances
            WindowHelper.ResizeFont(lab_instances, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(lab_instances_widthfix, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(bo_instances_save, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_save, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_instances_save, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(bo_instances_cancel, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_cancel, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_instances_cancel, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(scroll_instances, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(bo_instances_icon, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_icon_arrow, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(img_instances_icon, _heightMultiplier, _widthMultiplier);

            
            WindowHelper.Resize(bo_instances_iconlist, _heightMultiplier, _widthMultiplier);
            listbox_icons.Resources["IconHeight"] = double.Parse(listbox_icons.Resources["IconHeight"].ToString()) * _heightMultiplier;
            listbox_icons.Resources["IconWidth"] = double.Parse(listbox_icons.Resources["IconWidth"].ToString()) * _widthMultiplier;

            WindowHelper.Resize(grid_instances_name, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_name, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_name_placeholder, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(tb_instances_name, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(grid_instances_resolution, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_resolution, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_resolution_icon, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(tb_instances_resolution_x, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_resolution_x_placeholder, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_resolution_centericon, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(tb_instances_resolution_y, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_resolution_y_placeholder, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(grid_instances_gamedir, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_gamedir, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_gamedir_placeholder, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(tb_instances_gamedir, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_instances_gamedir, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(grid_instances_javadir, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_javadir, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_javadir_placeholder, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(tb_instances_javadir, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_instances_javadir, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(grid_instances_jvm, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_jvm, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_jvm_placeholder, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(tb_instances_jvm, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(grid_instances_version, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_version, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(cb_instances_version_type, _heightMultiplier, _widthMultiplier);
            cb_instances_version_type.Resources["VersionTypeListFontSize"] = double.Parse(cb_instances_version_type.Resources["VersionTypeListFontSize"].ToString()) * _widthMultiplier;
            WindowHelper.Resize(cb_instances_version, _heightMultiplier, _widthMultiplier);
            cb_instances_version.Resources["VersionListFontSize"] = double.Parse(cb_instances_version.Resources["VersionListFontSize"].ToString()) * _widthMultiplier;
            WindowHelper.ResizeFont(checkb_instances_version_releases, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(checkb_instances_version_betas, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(checkb_instances_version_snapshots, _heightMultiplier, _widthMultiplier);
            #endregion
            #endregion

            RefreshInstances();
            LoadVersions();
            listbox_icons.DataContext = ProfileIcon.Icons;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshAccount();
        }

        #region Functions
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
            if (profiles.Count <= 2)
                listbox_launchinstances.Resources["Alternation"] = profiles.Count;
            else
                listbox_launchinstances.Resources["Alternation"] = 2;
  
            if (profiles.TryGetValue(settings.SelectedProfile, out Profile selectedProfile))
                SelectedProfile = selectedProfile;
            else
                SelectedProfile = settings.Profiles.Values.ToList().ElementAt(0);

            listbox_launchinstances.SelectedIndex = profiles.Values.ToList().IndexOf(SelectedProfile);
            listbox_launchinstances.ScrollIntoView(listbox_launchinstances.SelectedItem);
            lab_selected_profile.Content = SelectedProfile.Name.ToLower();

        }
        
        private void LoadVersions()
        {
            #region Vanilla
            List<IVersion> localVersions = new List<IVersion>();



            VersionDic.Add("vanilla", localVersions);
            #endregion

            #region Forge & NeoForge

            #endregion

            #region Fabric

            #endregion

            #region Quilt

            #endregion
        }

        private void RefreshVersions(string versionType)
        {

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

        #region Main
        #region Logout Button
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

        private void LaunchLogout_MouseEnter(object sender, MouseEventArgs e)
        {
            gr_account.Background = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
            gr_account.BorderBrush = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
        }

        private void LaunchLogout_MouseLeave(object sender, MouseEventArgs e)
        {
            gr_account.Background = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
            gr_account.BorderBrush = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
        }
        #endregion

        #region New Instance Button
        private async void NewInstance_Click(object sender, RoutedEventArgs e)
        {
            bo_instances.IsEnabled = true;
            bo_instances.Visibility = Visibility.Visible;
        }

        private void NewInstance_MouseEnter(object sender, MouseEventArgs e)
        {
            bo_new_instance.Background = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
            bo_new_instance.BorderBrush = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
        }

        private void NewInstance_MouseLeave(object sender, MouseEventArgs e)
        {
            bo_new_instance.Background = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
            bo_new_instance.BorderBrush = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
        }
        #endregion

        #region Language Button
        private async void Language_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Language_MouseEnter(object sender, MouseEventArgs e)
        {
            bo_language.Background = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
            bo_language.BorderBrush = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
        }

        private void Language_MouseLeave(object sender, MouseEventArgs e)
        {
            bo_language.Background = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
            bo_language.BorderBrush = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
        }
        #endregion

        #region Launcher New Instance
        private void LauncherNew_MouseEnter(object sender, MouseEventArgs e)
        {
            //bo_launcher_new.Background = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
            //bo_launcher_new.BorderBrush = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
        }

        private void LauncherNew_MouseLeave(object sender, MouseEventArgs e)
        {
            //bo_launcher_new.Background = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
            //bo_launcher_new.BorderBrush = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
        }

        #endregion


        private readonly double _launchMaxStep = 4;
        private async void LaunchPlay_Click(object sender, RoutedEventArgs e)
        {
            bo_launch_play.IsEnabled = false;
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
                                    // TODO
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

                        string librarySizeCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
                        if (!Directory.Exists(librarySizeCacheDir))
                            Directory.CreateDirectory(librarySizeCacheDir);
                        string librarySizeCachePath = Path.Combine(librarySizeCacheDir, $"{version}.json");
                        // Check the version json file
                        if (!File.Exists(versionJsonPath))
                        {
                            pb_status.Value = 2d / _launchMaxStep * 100;
                            lab_install.Content = $"Downloading the version json file...";
                            using (var client = new HttpClient())
                            {
                                string resultJson = await client.GetStringAsync(mcVersion.Url);
                                JObject localObj = JObject.Parse(resultJson);
                                int localLibrarySize = 0;
                               
                                JArray localjArray = JArray.Parse(localObj["libraries"].ToString(Formatting.None));
                                // Check the libraries
                                foreach (var jToken in localjArray)
                                {
                                    localLibrarySize += int.Parse(jToken["downloads"]["artifact"]["size"].ToString());
                                }
                                localObj.Add("librarySize", localLibrarySize);
                                await File.WriteAllTextAsync(librarySizeCachePath, localLibrarySize.ToString());
                                await File.WriteAllTextAsync(versionJsonPath, localObj.ToString(Formatting.None));
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

                        // Download Assets
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
                            int libraryTotalSize = int.Parse(await File.ReadAllTextAsync(librarySizeCachePath));
                            double libraryDownloadedSize = 0;
                            foreach (var jToken in jArray)
                            {
                                double percent = (libraryDownloadedSize / (double)libraryTotalSize) * 100;
                                lab_install.Content = $"Downloading the {jToken["name"]} library... {percent.ToString("0.00")}%";
                                pb_status.Value = percent;
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
                                libraryDownloadedSize += int.Parse(jToken["downloads"]["artifact"]["size"].ToString());
                            }
                        }

                        libraryBundle += $"{versionJarPath}";
                        pb_status.Value = 4d / _launchMaxStep * 100;
                        lab_install.Content = $"Launching the game...";
                        break;
                    }
                case API.Enums.EProfileKind.FORGE: // TODO
                    {
                        // TODO
                        break;
                    }
                case API.Enums.EProfileKind.FABRIC: // TODO
                    {
                        // TODO
                        break;
                    }
                case API.Enums.EProfileKind.QUILT: // TODO
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
            argumnets.Add($"--gameDir {gameDir}");
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

            bo_launch_play.IsEnabled = true;
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

        private async void listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LauncherSettings? settings = IOHelper.GetLauncherSettings();
            if (settings != null)
            {
                var p = settings.Profiles.ElementAt(listbox_launchinstances.SelectedIndex);
                lab_selected_profile.Content = p.Value.Name.ToLower();
                settings.SelectedProfile = p.Key;
                await JsonHelper.WriteJsonFile(Path.Combine(IOHelper.MainDirectory, "launcher.json"), settings);
            }
        }
        #endregion

        #region Instances
        #region Variables
        public string SelectedIcon { get; set; }
        public Dictionary<string, List<IVersion>> VersionDic {  get; set; }
        #endregion

        private void InstancesSave_Click(object sender, RoutedEventArgs e)
        {

        }
        private void InstancesCancel_Click(object sender, RoutedEventArgs e)
        {
            bo_instances.IsEnabled = false;
            bo_instances.Visibility = Visibility.Hidden;
        }

        #region Icon Edit
        private void InstancesIcon_Click(object sender, RoutedEventArgs e)
        {
            if (bo_instances_iconlist.IsEnabled)
            {
                bo_instances_iconlist.IsEnabled = false;
                bo_instances_iconlist.Visibility = Visibility.Collapsed;
                lab_instances_icon_arrow.Content = "\uf078";
            }
            else
            {
                bo_instances_iconlist.IsEnabled = true;
                bo_instances_iconlist.Visibility = Visibility.Visible;
                lab_instances_icon_arrow.Content = "\uf077";
            }
        }

        private void InstancesIcon_MouseEnter(object sender, MouseEventArgs e)
        {
            bo_instances_icon.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#33000000"));
            bo_instances_icon.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#33000000"));
        }

        private void InstancesIcon_MouseLeave(object sender, MouseEventArgs e)
        {
            bo_instances_icon.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000"));
            bo_instances_icon.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000"));
        }

        private void InstancesIconSelect_Click(object sender, RoutedEventArgs e)
        {
            if (bo_instances_iconlist.IsEnabled)
            {
                bo_instances_iconlist.IsEnabled = false;
                bo_instances_iconlist.Visibility = Visibility.Collapsed;
                lab_instances_icon_arrow.Content = "\uf078";
                Button btn = (Button)sender;
                Image image = (Image)btn.Content;

                string rawPath = ((BitmapFrame)image.Source).Decoder.ToString();
                SelectedIcon= rawPath.Remove(0, rawPath.IndexOf("assets"));
                img_instances_icon.Source = image.Source;
            }
        }

        #endregion

        private void InstancesName_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool enab = string.IsNullOrEmpty(tb_instances_name.Text);
            lab_instances_name_placeholder.IsEnabled = enab;
            lab_instances_name_placeholder.Visibility = enab ? Visibility.Visible : Visibility.Hidden;
        }

        private void InstancesJVM_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool enab = string.IsNullOrEmpty(tb_instances_jvm.Text);
            lab_instances_jvm_placeholder.IsEnabled = enab;
            lab_instances_jvm_placeholder.Visibility = enab ? Visibility.Visible : Visibility.Hidden;
        }

        private void InstancesGamedir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.InitialDirectory = IOHelper.InstancesDir;
            dialog.ShowNewFolderButton = true;
            dialog.Description = "Select game directory";
            dialog.UseDescriptionForTitle = true;
            var result = dialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    {
                        tb_instances_gamedir.Text = dialog.SelectedPath;
                        lab_instances_gamedir_placeholder.IsEnabled = false;
                        lab_instances_gamedir_placeholder.Visibility = Visibility.Hidden;
                        break;
                    }
                default:
                    {

                        break;
                    }
            }
        }

        private void InstancesJavadir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.Description = "Select java bin directory";
            dialog.UseDescriptionForTitle = true;
            var result = dialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    {
                        tb_instances_javadir.Text = dialog.SelectedPath;
                        lab_instances_javadir_placeholder.IsEnabled = false;
                        lab_instances_javadir_placeholder.Visibility = Visibility.Hidden;
                        break;
                    }
                default:
                    {

                        break;
                    }
            }
        }

        private void InstancesResolutionX_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool enab = string.IsNullOrEmpty(tb_instances_resolution_x.Text);
            lab_instances_resolution_x_placeholder.IsEnabled = enab;
            lab_instances_resolution_x_placeholder.Visibility = enab ? Visibility.Visible : Visibility.Hidden;
        }

        private void InstancesResolutionY_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool enab = string.IsNullOrEmpty(tb_instances_resolution_y.Text);
            lab_instances_resolution_y_placeholder.IsEnabled = enab;
            lab_instances_resolution_y_placeholder.Visibility = enab ? Visibility.Visible : Visibility.Hidden;
        }

        private void InstancesResolutionX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(x => char.IsNumber(x));
        }

        private void InstancesResolutionY_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(x => char.IsNumber(x));
        }

        private void InstanceVersionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_instances_version_type.SelectedValue == null)
                return;

            string version = cb_instances_version_type.SelectedValue.ToString().ToLower();
            switch (version)
            {
                case "vanilla":
                    {
                        checkb_instances_version_releases.IsEnabled = true;
                        checkb_instances_version_releases.Visibility = Visibility.Visible;
                        checkb_instances_version_snapshots.IsEnabled = true;
                        checkb_instances_version_snapshots.Visibility = Visibility.Visible;
                        checkb_instances_version_betas.IsEnabled = true;
                        checkb_instances_version_betas.Visibility = Visibility.Visible;
                        break;
                    }
                case "forge":
                case "neoforge":
                    {
                        checkb_instances_version_releases.IsEnabled = false;
                        checkb_instances_version_releases.Visibility = Visibility.Hidden;
                        checkb_instances_version_snapshots.IsEnabled = false;
                        checkb_instances_version_snapshots.Visibility = Visibility.Hidden;
                        checkb_instances_version_betas.IsEnabled = false;
                        checkb_instances_version_betas.Visibility = Visibility.Hidden;
                        break;
                    }
                case "fabric":
                    {
                        checkb_instances_version_releases.IsEnabled = true;
                        checkb_instances_version_releases.Visibility = Visibility.Visible;
                        checkb_instances_version_snapshots.IsEnabled = true;
                        checkb_instances_version_snapshots.Visibility = Visibility.Visible;
                        checkb_instances_version_betas.IsEnabled = false;
                        checkb_instances_version_betas.Visibility = Visibility.Hidden;
                        break;
                    }
                case "quilt":
                    {
                        checkb_instances_version_releases.IsEnabled = true;
                        checkb_instances_version_releases.Visibility = Visibility.Visible;
                        checkb_instances_version_snapshots.IsEnabled = true;
                        checkb_instances_version_snapshots.Visibility = Visibility.Visible;
                        checkb_instances_version_betas.IsEnabled = false;
                        checkb_instances_version_betas.Visibility = Visibility.Hidden;
                        break;
                    }
            }
            RefreshVersions(version);
        }

        private void InstanceVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_instances_version.SelectedValue == null)
                return;
        }
        #endregion


    }
}

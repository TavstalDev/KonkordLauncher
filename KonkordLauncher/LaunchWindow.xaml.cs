using KonkordLibrary.Enums;
using KonkordLibrary.Helpers;
using KonkordLibrary.Models;
using KonkordLibrary.Models.Minecraft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
        private string? EditedProfileKey {  get; set; }
        private Profile? EditedProfile {  get; set; }
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

        public LaunchWindow(string? profileKey)
        {
            InitializeComponent();
            Loaded += Window_Loaded;
            #region Resize
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

            WindowHelper.Resize(grid_instances_memory, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_memory, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(cb_instances_memory, _heightMultiplier, _widthMultiplier);
            cb_instances_memory.Resources["MemoryListFontSize"] = double.Parse(cb_instances_memory.Resources["MemoryListFontSize"].ToString()) * _widthMultiplier;

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

            WindowHelper.Resize(grid_instances_launchopt, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_launchopt, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(cb_instances_launchopt, _heightMultiplier, _widthMultiplier);
            cb_instances_launchopt.Resources["LaunchOptListFontSize"] = double.Parse(cb_instances_launchopt.Resources["LaunchOptListFontSize"].ToString()) * _widthMultiplier;
            #endregion
            #endregion

            listbox_icons.DataContext = ProfileIcon.Icons;
            RefreshInstances();
            // Fill Version ComboBox
            FillVersionComboBox();
            // Fill Memory ComboBox
            #region Memory ComboBox
            var gcMemoryInfo = GC.GetGCMemoryInfo();
            var installedMemory = gcMemoryInfo.TotalAvailableMemoryBytes;
            // it will give the size of memory in GB
            int physicalMemory = (int)((double)installedMemory / 1048576.0 / 1024.0);

            List<int> memoryElements = new List<int>();
            for (int i = 0; i < physicalMemory; i++)
            {
                memoryElements.Add(i + 1);
            }
            cb_instances_memory.DataContext = memoryElements;
            cb_instances_memory.SelectedIndex = 0;
            #endregion
            // Fill LaunchOpt ComboBox
            #region LaunchOpt ComboBox
            List<object> launchOptList = new List<object>()
            {
                new { Tag = 0, Text = "Reopen on game close" },
                new { Tag = 1, Text = "Close on game start"},
                new { Tag = 2, Text = "Keep open" }
            };
            cb_instances_launchopt.DataContext = launchOptList;
            cb_instances_launchopt.SelectedIndex = 0;
            #endregion
            #endregion

            #region Load Defaults
            tb_instances_jvm.Text = "-XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=16M -Djava.net.preferIPv4Stack=true";
            if (profileKey == null)
            {
                RefreshDropdownVersions("vanilla");
                return;
            }

            LauncherSettings? launcherSettings = IOHelper.GetLauncherSettings();
            if (launcherSettings == null)
            {
                RefreshDropdownVersions("vanilla");
                return;
            }

            if (!launcherSettings.Profiles.TryGetValue(profileKey, out Profile editedProfile))
            {
                RefreshDropdownVersions("vanilla");
                return;
            }

            EditedProfile = editedProfile;
            EditedProfileKey = profileKey;
            switch (editedProfile.Kind)
            {
                case EProfileKind.VANILLA:
                    {
                        RefreshDropdownVersions("vanilla");
                        cb_instances_version.SelectedItem = EditedProfile.VersionId;
                        break;
                    }
                case EProfileKind.FORGE:
                    {
                        RefreshDropdownVersions("forge");
                        cb_instances_version.SelectedItem = EditedProfile.VersionId;
                        break;
                    }
                case EProfileKind.FABRIC:
                    {
                        RefreshDropdownVersions("fabric");
                        cb_instances_version.SelectedItem = EditedProfile.VersionId;
                        break;
                    }
                case EProfileKind.QUILT:
                    {
                        RefreshDropdownVersions("quilt");
                        cb_instances_version.SelectedItem = EditedProfile.VersionId;
                        break;
                    }
            }

            tb_instances_gamedir.Text = EditedProfile.GameDirectory ?? "";
            tb_instances_javadir.Text = EditedProfile.JavaPath ?? "";
            tb_instances_name.Text = EditedProfile.Name;
            tb_instances_jvm.Text = EditedProfile.JVMArgs;
            if (EditedProfile.Resolution != null)
            {
                tb_instances_resolution_x.Text = EditedProfile.Resolution.X.ToString();
                tb_instances_resolution_y.Text = EditedProfile.Resolution.Y.ToString();
            }
            cb_instances_launchopt.SelectedIndex = (int)EditedProfile.LauncherVisibility;
            cb_instances_memory.SelectedItem = EditedProfile.Memory;
            #endregion
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshAccount();
        }

        #region Functions
        /// <summary>
        /// Asynchronously refreshes the displayed account information, such as icon and name.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        private async Task RefreshAccount()
        {
            AccountData? accountData = await JsonHelper.ReadJsonFileAsync<AccountData>(System.IO.Path.Combine(IOHelper.MainDirectory, "accounts.json"));
            if (accountData == null)
                return;

            var account = accountData.Accounts[accountData.SelectedAccountId];
            if (account == null)
                return;

            la_account_name.Content = account.DisplayName;
            switch (account.Type)
            {
                case EAccountType.OFFLINE:
                    {
                        la_account_type.Content = "Offline account";
                        break;
                    }
                case EAccountType.MICROSOFT:
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

        /// <summary>
        /// Refreshes the Minecraft instances user interface.
        /// </summary>
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

        /// <summary>
        /// Fills the version ComboBox with available versions.
        /// </summary>
        private void FillVersionComboBox()
        {
            if (VersionDic != null)
                return;

            VersionDic = new Dictionary<string, List<VersionBase>>();

            #region Vanilla
            List<VersionBase> localVersions = new List<VersionBase>();

            var manifest = JsonConvert.DeserializeObject<VersionManifest>(File.ReadAllText(Path.Combine(IOHelper.ManifestDir, "vanillaManifest.json")));
            foreach (var v in manifest.Versions)
            {
                localVersions.Add(new VersionBase(v.Id, v.GetVersionBaseType()));
            }

            VersionDic.Add("vanilla", localVersions);
            #endregion

            #region Forge & NeoForge
            localVersions = new List<VersionBase>();

            string raw = File.ReadAllText(Path.Combine(IOHelper.ManifestDir, "forgeManifest.json"));
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(raw);
            JObject jObj = JObject.Parse(JsonConvert.SerializeXmlNode(doc));
            foreach (var v in jObj["metadata"]["versioning"]["versions"]["version"].ToList())
            {
                localVersions.Add(new VersionBase(v.ToString(), EVersionType.RELEASE));
            }

            VersionDic.Add("forge", localVersions);
            #endregion

            #region Fabric
            localVersions = new List<VersionBase>();
            raw = File.ReadAllText(Path.Combine(IOHelper.ManifestDir, "fabricManifest.json"));
            JArray jArray = JArray.Parse(raw);
            foreach (var token in jArray)
            {
                localVersions.Add(new VersionBase(token["version"].ToString(), bool.Parse(token["stable"].ToString()) ? EVersionType.RELEASE : EVersionType.SNAPSHOT));
            }

            VersionDic.Add("fabric", localVersions);
            #endregion

            #region Quilt
            localVersions = new List<VersionBase>();
            raw = File.ReadAllText(Path.Combine(IOHelper.ManifestDir, "quiltManifest.json"));
            jObj = JObject.Parse(raw);
            foreach (var token in jObj["loader"].ToList())
            {
                localVersions.Add(new VersionBase(token["version"].ToString(), token["maven"].ToString().Contains("beta") ? EVersionType.SNAPSHOT : EVersionType.RELEASE));
            }

            VersionDic.Add("quilt", localVersions);
            #endregion
        }

        /// <summary>
        /// Refreshes the dropdown versions based on the specified version type.
        /// </summary>
        /// <param name="versionType">The type of version to refresh (e.g., "vanilla", "forge", etc.).</param>
        private void RefreshDropdownVersions(string versionType)
        {
            if (checkb_instances_version_betas == null || checkb_instances_version_releases == null || checkb_instances_version_snapshots == null)
                return;

            if (versionType == "neoforge")
                versionType = "forge";

            bool showReleases = true;
            if (checkb_instances_version_releases.IsEnabled)
                showReleases = checkb_instances_version_releases.IsChecked.Value;
            bool showSnapshots = true;
            if (checkb_instances_version_snapshots.IsEnabled)
                showSnapshots = checkb_instances_version_snapshots.IsChecked.Value;
            bool showOldBetas = true;
            if (checkb_instances_version_betas.IsEnabled)
                showOldBetas = checkb_instances_version_betas.IsChecked.Value;

            cb_instances_version.DataContext = VersionDic[versionType].FindAll(x => (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas));
            cb_instances_version.SelectedIndex = 0;
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
        /// <summary>
        /// Handles the click event of the logout button in the launch interface.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private async void LaunchLogout_Click(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(IOHelper.MainDirectory, "accounts.json");
            AccountData? accountData = await JsonHelper.ReadJsonFileAsync<AccountData>(path);
            if (accountData == null)
                return;

            accountData.SelectedAccountId = string.Empty;
            await JsonHelper.WriteJsonFileAsync(path, accountData);
            AuthWindow window = new AuthWindow();
            window.Show();
            this.Close();
        }

        /// <summary>
        /// Handles the mouse enter event of the logout button in the launch interface.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void LaunchLogout_MouseEnter(object sender, MouseEventArgs e)
        {
            gr_account.Background = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
            gr_account.BorderBrush = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
        }

        /// <summary>
        /// Handles the mouse leave event of the logout button in the launch interface.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void LaunchLogout_MouseLeave(object sender, MouseEventArgs e)
        {
            gr_account.Background = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
            gr_account.BorderBrush = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
        }
        #endregion

        #region New Instance Button
        /// <summary>
        /// Handles the click event of the "New Instance" button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void NewInstance_Click(object sender, RoutedEventArgs e)
        {
            bo_instances.IsEnabled = true;
            bo_instances.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the mouse enter event of the "New Instance" button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void NewInstance_MouseEnter(object sender, MouseEventArgs e)
        {
            bo_new_instance.Background = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
            bo_new_instance.BorderBrush = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
        }

        /// <summary>
        /// Handles the mouse leave event of the "New Instance" button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void NewInstance_MouseLeave(object sender, MouseEventArgs e)
        {
            bo_new_instance.Background = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
            bo_new_instance.BorderBrush = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
        }
        #endregion

        #region Language Button
        /// <summary>
        /// Handles the click event of the language selection button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Language_Click(object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// Handles the mouse enter event of the language selection button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Language_MouseEnter(object sender, MouseEventArgs e)
        {
            bo_language.Background = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
            bo_language.BorderBrush = new SolidColorBrush(Color.FromScRgb(0.15f, 0, 0, 0));
        }

        /// <summary>
        /// Handles the mouse leave event of the language selection button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Language_MouseLeave(object sender, MouseEventArgs e)
        {
            bo_language.Background = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
            bo_language.BorderBrush = new SolidColorBrush(Color.FromScRgb(0f, 0, 0, 0));
        }
        #endregion

        private readonly double _launchMaxStep = 4;
        /// <summary>
        /// Handles the click event of the "Play" button in the launch interface.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private async void LaunchPlay_Click(object sender, RoutedEventArgs e)
        {
            // Disable play button
            bo_launch_play.IsEnabled = false;
            btn_launch_play.IsEnabled = false;

            #region Check requirements
            AccountData? accountData = await  JsonHelper.ReadJsonFileAsync<AccountData>(System.IO.Path.Combine(IOHelper.MainDirectory, "accounts.json"));
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
            #endregion

            #region Declare variables
            string version = string.Empty;
            string versionDirectory = string.Empty;
            string versionJsonPath = string.Empty;
            string versionJarPath = string.Empty;
            string gameDir = string.Empty;
            string libraryBundle = string.Empty;
            string assetIndex = string.Empty;
            #endregion

            // Enable install progressbar TODO: This should be reworked later, but at the moment it's unimportant
            bo_install.IsEnabled = true;
            bo_install.Visibility = Visibility.Visible;
            // Reset progressbar value
            pb_status.Value = 0d;
            lab_install.Content = $"Reading the manifest file...";
            List<string> argumnets = new List<string>();

            switch (selectedProfile.Kind)
            {
                case EProfileKind.VANILLA:
                    {
                        // Read the Version Manifest
                        VersionManifest? manifest = await JsonHelper.ReadJsonFileAsync<VersionManifest>(System.IO.Path.Combine(IOHelper.ManifestDir, "vanillaManifest.json"));
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
                            case EProfileType.LATEST_RELEASE:
                                {
                                    version = manifest.Latest.Release;
                                    versionDirectory = System.IO.Path.Combine(IOHelper.VersionsDir, manifest.Latest.Release);
                                    versionJsonPath = System.IO.Path.Combine(versionDirectory, $"{manifest.Latest.Release}.json");
                                    versionJarPath = System.IO.Path.Combine(versionDirectory, $"{manifest.Latest.Release}.jar");
                                    gameDir = System.IO.Path.Combine(IOHelper.InstancesDir, $"{manifest.Latest.Release}");
                                    break;
                                }
                            case EProfileType.LATEST_SNAPSHOT:
                                {
                                    version = manifest.Latest.Snapshot;
                                    versionDirectory = System.IO.Path.Combine(IOHelper.VersionsDir, manifest.Latest.Snapshot);
                                    versionJsonPath = System.IO.Path.Combine(versionDirectory, $"{manifest.Latest.Snapshot}.json");
                                    versionJarPath = System.IO.Path.Combine(versionDirectory, $"{manifest.Latest.Snapshot}.jar");
                                    gameDir = System.IO.Path.Combine(IOHelper.InstancesDir, $"{manifest.Latest.Snapshot}");
                                    break;
                                }
                            case EProfileType.CUSTOM:
                                {
                                    version = selectedProfile.VersionId;
                                    versionDirectory = System.IO.Path.Combine(IOHelper.VersionsDir, selectedProfile.VersionId);
                                    versionJsonPath = System.IO.Path.Combine(versionDirectory, $"{selectedProfile.VersionId}.json");
                                    versionJarPath = System.IO.Path.Combine(versionDirectory, $"{selectedProfile.VersionId}.jar");
                                    gameDir = selectedProfile.GameDirectory;
                                    break;
                                }
                            case EProfileType.KONKORD_CREATE:
                            case EProfileType.KONKORD_VANILLAPLUS:
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
                case EProfileKind.FORGE:
                    {
                        // TODO 
                        break;
                    }
                case EProfileKind.FABRIC:
                    {
                        // TODO
                        break;
                    }
                case EProfileKind.QUILT:
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

            #region Finish and Launch Minecraft
            // Enable play button
            bo_launch_play.IsEnabled = true;
            btn_launch_play.IsEnabled = true;
            // Disable progressbar
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
                case ELaucnherVisibility.HIDE_AND_REOPEN_ON_GAME_CLOSE:
                    {
                        this.Hide();
                        await p.WaitForExitAsync();
                        this.Show();
                        break;
                    }
                case ELaucnherVisibility.CLOSE_ON_GAME_START:
                    {
                        this.Close();
                        break;
                    }
                case ELaucnherVisibility.KEEP_OPEN:
                    {
                        // Just do nothing
                        break;
                    }
            }
            #endregion
        }

        /// <summary>
        /// Handles the selection changed event of the ListBox.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private async void listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listbox_launchinstances.SelectedIndex < 0)
                return;

            LauncherSettings? settings = IOHelper.GetLauncherSettings();
            if (settings != null)
            {
                var p = settings.Profiles.ElementAt(listbox_launchinstances.SelectedIndex);
                lab_selected_profile.Content = p.Value.Name.ToLower();
                settings.SelectedProfile = p.Key;
                await JsonHelper.WriteJsonFileAsync(Path.Combine(IOHelper.MainDirectory, "launcher.json"), settings);
            }
        }
        #endregion

        #region Instances
        #region Variables
        public string SelectedIcon { get; set; }
        public Dictionary<string, List<VersionBase>> VersionDic {  get; set; }
        #endregion

        /// <summary>
        /// Handles the click event of the instances save button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private async void InstancesSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb_instances_name.Text))
            {
                NotificationHelper.SendError("You must provide the name of the instance.", "Error");
                return;
            }

            if (cb_instances_version.SelectedIndex < 0)
            {
                NotificationHelper.SendError("You must select a version.", "Error");
                return;
            }
            EProfileKind profileKind = (EProfileKind)cb_instances_version_type.SelectedIndex;
            ELaucnherVisibility laucnherVisibility = (ELaucnherVisibility)cb_instances_launchopt.SelectedIndex;
            Resolution? resolution = null;
            if (!(string.IsNullOrEmpty(tb_instances_resolution_x.Text) && string.IsNullOrEmpty(tb_instances_resolution_y.Text)))
                resolution = new Resolution()
                {
                    X = int.Parse(tb_instances_resolution_x.Text),
                    Y = int.Parse(tb_instances_resolution_y.Text),
                    IsFullScreen = false
                };

            try
            {
                Profile profile = new()
                {
                    Name = tb_instances_name.Text,
                    GameDirectory = tb_instances_gamedir.Text ?? "",
                    Icon = img_instances_icon.Source.ToString(),
                    JavaPath = tb_instances_javadir.Text ?? "",
                    Kind = profileKind,
                    LauncherVisibility = laucnherVisibility,
                    Memory = (int)cb_instances_memory.SelectedItem,
                    Resolution = resolution,
                    Type = EProfileType.CUSTOM,
                    VersionId = cb_instances_version.Text,
                    JVMArgs = tb_instances_jvm.Text ?? "",
                };

                LauncherSettings? settings = IOHelper.GetLauncherSettings();
                if (settings == null)
                    return;

                if (settings.Profiles.ContainsKey(EditedProfileKey ?? ""))
                {
                    settings.Profiles[EditedProfileKey] = profile;
                }
                else
                {
                    settings.Profiles.Add(Guid.NewGuid().ToString(), profile);
                }
                await JsonHelper.WriteJsonFileAsync(Path.Combine(IOHelper.MainDirectory, "launcher.json"), settings);
                RefreshInstances();
                bo_instances.IsEnabled = false;
                bo_instances.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Handles the click event of the instances cancel button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InstancesCancel_Click(object sender, RoutedEventArgs e)
        {
            bo_instances.IsEnabled = false;
            bo_instances.Visibility = Visibility.Hidden;
        }

        #region Icon Edit
        /// <summary>
        /// Handles the click event of the instances icon button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
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

        /// <summary>
        /// Handles the mouse enter event of the instances icon button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InstancesIcon_MouseEnter(object sender, MouseEventArgs e)
        {
            bo_instances_icon.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#33000000"));
            bo_instances_icon.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#33000000"));
        }

        /// <summary>
        /// Handles the mouse leave event of the instances icon button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InstancesIcon_MouseLeave(object sender, MouseEventArgs e)
        {
            bo_instances_icon.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000"));
            bo_instances_icon.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000"));
        }

        /// <summary>
        /// Handles the click event of the instances icon select button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
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

        /// <summary>
        /// Handles the text changed event of the instances name TextBox.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InstancesName_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool enab = string.IsNullOrEmpty(tb_instances_name.Text);
            lab_instances_name_placeholder.IsEnabled = enab;
            lab_instances_name_placeholder.Visibility = enab ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Handles the text changed event of the instances JVM TextBox.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InstancesJVM_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool enab = string.IsNullOrEmpty(tb_instances_jvm.Text);
            lab_instances_jvm_placeholder.IsEnabled = enab;
            lab_instances_jvm_placeholder.Visibility = enab ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Handles the text changed event of the instances Java directory TextBox.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InstancesJavaDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool enab = string.IsNullOrEmpty(tb_instances_javadir.Text);
            lab_instances_javadir_placeholder.IsEnabled = enab;
            lab_instances_javadir_placeholder.Visibility = enab ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Handles the text changed event of the instances game directory TextBox.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InstancesGameDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool enab = string.IsNullOrEmpty(tb_instances_gamedir.Text);
            lab_instances_gamedir_placeholder.IsEnabled = enab;
            lab_instances_gamedir_placeholder.Visibility = enab ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Handles the click event of the instances game directory button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
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

        /// <summary>
        /// Handles the click event of the instances Java directory button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
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

        /// <summary>
        /// Handles the text changed event of the instances X resolution TextBox.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InstancesResolutionX_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool enab = string.IsNullOrEmpty(tb_instances_resolution_x.Text);
            lab_instances_resolution_x_placeholder.IsEnabled = enab;
            lab_instances_resolution_x_placeholder.Visibility = enab ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Handles the text changed event of the instances Y resolution TextBox.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InstancesResolutionY_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool enab = string.IsNullOrEmpty(tb_instances_resolution_y.Text);
            lab_instances_resolution_y_placeholder.IsEnabled = enab;
            lab_instances_resolution_y_placeholder.Visibility = enab ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Handles the preview text input event of the instances X resolution TextBox.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InstancesResolutionX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(x => char.IsNumber(x));
        }

        /// <summary>
        /// Handles the preview text input event of the instances Y resolution TextBox.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InstancesResolutionY_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(x => char.IsNumber(x));
        }

        /// <summary>
        /// Handles the selection changed event of the instance version type ComboBox.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
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
            RefreshDropdownVersions(version);
        }

        /// <summary>
        /// Handles the checked event of the instances version CheckBox.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InstancesVersion_Checked(object sender, RoutedEventArgs e)
        {
            RefreshDropdownVersions(cb_instances_version_type.SelectedValue.ToString().ToLower());
        }
        #endregion
    }
}

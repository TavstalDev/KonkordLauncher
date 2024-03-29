using KonkordLibrary.Enums;
using KonkordLibrary.Helpers;
using KonkordLibrary.Managers;
using KonkordLibrary.Models;
using KonkordLibrary.Models.Fabric;
using KonkordLibrary.Models.Forge;
using KonkordLibrary.Models.GameManager;
using KonkordLibrary.Models.Minecraft;
using KonkordLibrary.Models.Minecraft.Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
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

            WindowHelper.Resize(bo_launch_progress, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(bo_launch_progress_bar, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_launch_progress, _heightMultiplier, _widthMultiplier);

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
            WindowHelper.Resize(cb_instances_mc_version, _heightMultiplier, _widthMultiplier);
            cb_instances_mc_version.Resources["VersionListFontSize"] = double.Parse(cb_instances_mc_version.Resources["VersionListFontSize"].ToString()) * _widthMultiplier;
            WindowHelper.Resize(cb_instances_mcmod_version, _heightMultiplier, _widthMultiplier);
            cb_instances_mcmod_version.Resources["VersionListFontSize"] = double.Parse(cb_instances_mcmod_version.Resources["VersionListFontSize"].ToString()) * _widthMultiplier;
            WindowHelper.Resize(cb_instances_mod_version, _heightMultiplier, _widthMultiplier);
            cb_instances_mod_version.Resources["VersionListFontSize"] = double.Parse(cb_instances_mod_version.Resources["VersionListFontSize"].ToString()) * _widthMultiplier;
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
                        cb_instances_mc_version.SelectedItem = EditedProfile.VersionId;
                        cb_instances_mc_version.IsEnabled = true;
                        cb_instances_mcmod_version.IsEnabled = false;
                        cb_instances_mod_version.IsEnabled = false;
                        cb_instances_mc_version.Visibility = Visibility.Visible;
                        cb_instances_mcmod_version.Visibility = Visibility.Hidden;
                        cb_instances_mod_version.Visibility = Visibility.Hidden;
                        break;
                    }
                case EProfileKind.FORGE:
                    {
                        RefreshDropdownVersions("forge");
                        cb_instances_mcmod_version.SelectedItem = EditedProfile.VersionVanillaId;
                        cb_instances_mod_version.SelectedItem = EditedProfile.VersionId.Split('-')[1];
                        cb_instances_mc_version.IsEnabled = false;
                        cb_instances_mcmod_version.IsEnabled = true;
                        cb_instances_mod_version.IsEnabled = true;
                        cb_instances_mc_version.Visibility = Visibility.Hidden;
                        cb_instances_mcmod_version.Visibility = Visibility.Visible;
                        cb_instances_mod_version.Visibility = Visibility.Visible;
                        break;
                    }
                case EProfileKind.FABRIC:
                    {
                        RefreshDropdownVersions("fabric");
                        cb_instances_mcmod_version.SelectedItem = EditedProfile.VersionVanillaId;
                        cb_instances_mod_version.SelectedItem = EditedProfile.VersionId.Split('-')[1];
                        cb_instances_mc_version.IsEnabled = false;
                        cb_instances_mcmod_version.IsEnabled = true;
                        cb_instances_mod_version.IsEnabled = true;
                        cb_instances_mc_version.Visibility = Visibility.Hidden;
                        cb_instances_mcmod_version.Visibility = Visibility.Visible;
                        cb_instances_mod_version.Visibility = Visibility.Visible;
                        break;
                    }
                case EProfileKind.QUILT:
                    {
                        RefreshDropdownVersions("quilt");
                        cb_instances_mcmod_version.SelectedItem = EditedProfile.VersionVanillaId;
                        cb_instances_mod_version.SelectedItem = EditedProfile.VersionId.Split('-')[1];
                        cb_instances_mc_version.IsEnabled = false;
                        cb_instances_mcmod_version.IsEnabled = true;
                        cb_instances_mod_version.IsEnabled = true;
                        cb_instances_mc_version.Visibility = Visibility.Hidden;
                        cb_instances_mcmod_version.Visibility = Visibility.Visible;
                        cb_instances_mod_version.Visibility = Visibility.Visible;
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
        /// Fills the instanceVersion ComboBox with available versions.
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
                localVersions.Add(new VersionBase(v.Id, v.Id, v.GetVersionBaseType()));
            }

            VersionDic.Add("vanilla", localVersions);
            #endregion

            #region Forge & NeoForge
            localVersions = new List<VersionBase>();
            List <VersionBase> forgeVanilla = new List<VersionBase>();

            string raw = File.ReadAllText(Path.Combine(IOHelper.ManifestDir, "forgeManifest.json"));
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(raw);
            JObject jObj = JObject.Parse(JsonConvert.SerializeXmlNode(doc));
            foreach (var v in jObj["metadata"]["versioning"]["versions"]["version"].ToList())
            {
                string[] s = v.ToString().Split('-');
                localVersions.Add(new VersionBase(s[1], s[0], EVersionType.RELEASE));

                if (!forgeVanilla.Any(x => x.VanillaId == s[0]))
                    forgeVanilla.Add(new VersionBase(s[0], s[0], EVersionType.RELEASE));
            }

            VersionDic.Add("forgeVanilla", forgeVanilla);
            VersionDic.Add("forge", localVersions);
            #endregion

            #region Fabric

            localVersions = new List<VersionBase>();
            raw = File.ReadAllText(Path.Combine(IOHelper.ManifestDir, "fabricManifest.json"));
            jObj = JObject.Parse(raw);
            foreach (var token in jObj["game"].ToList())
            {
                localVersions.Add(new VersionBase(token["version"].ToString(), token["version"].ToString(), bool.Parse(token["stable"].ToString()) ? EVersionType.RELEASE : EVersionType.SNAPSHOT));
            }

            VersionDic.Add("fabricVanilla", localVersions);

            localVersions = new List<VersionBase>();
            foreach (var token in jObj["loader"].ToList())
            {
                localVersions.Add(new VersionBase(token["version"].ToString(), string.Empty, EVersionType.RELEASE));
            }

            VersionDic.Add("fabric", localVersions);
            #endregion

            #region Quilt
            localVersions = new List<VersionBase>();
            raw = File.ReadAllText(Path.Combine(IOHelper.ManifestDir, "quiltManifest.json"));
            jObj = JObject.Parse(raw);
            foreach (var token in jObj["game"].ToList())
            {
                localVersions.Add(new VersionBase(token["version"].ToString(), token["version"].ToString(), bool.Parse(token["stable"].ToString()) ? EVersionType.RELEASE : EVersionType.SNAPSHOT));
            }

            VersionDic.Add("quiltVanilla", localVersions);
            localVersions = new List<VersionBase>();
            foreach (var token in jObj["loader"].ToList())
            {
                localVersions.Add(new VersionBase(token["version"].ToString(), string.Empty, EVersionType.RELEASE));
            }

            VersionDic.Add("quilt", localVersions);
            #endregion
        }

        /// <summary>
        /// Refreshes the dropdown versions based on the specified instanceVersion type.
        /// </summary>
        /// <param name="versionType">The type of instanceVersion to refresh (e.g., "vanilla", "forge", etc.).</param>
        private void RefreshDropdownVersions(string versionType)
        {
            if (checkb_instances_version_betas == null || checkb_instances_version_releases == null || checkb_instances_version_snapshots == null)
                return;

            if (versionType == "neoforge")
                versionType = "forge";

            bool showReleases = true;
            if (checkb_instances_version_releases.IsEnabled)
                showReleases = checkb_instances_version_releases.IsChecked.Value;
            bool showSnapshots = false;
            if (checkb_instances_version_snapshots.IsEnabled)
                showSnapshots = checkb_instances_version_snapshots.IsChecked.Value;
            bool showOldBetas = false;
            if (checkb_instances_version_betas.IsEnabled)
                showOldBetas = checkb_instances_version_betas.IsChecked.Value;

            // Full Vanilla
            if (cb_instances_mc_version.IsEnabled)
            {
                cb_instances_mc_version.DataContext = VersionDic[versionType].FindAll(x => (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas)).Select(x => x.Id);
                cb_instances_mc_version.SelectedIndex = 0;
            }
            else // Moded
            {
                List<VersionBase> localVanillaList = new List<VersionBase>();
                List<VersionBase> localModList = new List<VersionBase>();
                switch (versionType)
                {
                    case "fabric":
                        {
                            localVanillaList = VersionDic["fabricVanilla"].FindAll(x => (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas));
                            cb_instances_mcmod_version.DataContext = localVanillaList.Select(x => x.Id);
                            cb_instances_mcmod_version.SelectedIndex = 0;

                            localModList = VersionDic[versionType];
                            cb_instances_mod_version.DataContext = localModList.Select(x => x.Id);
                            cb_instances_mod_version.SelectedIndex = 0;
                            return;
                        }
                    case "quilt":
                        {
                            localVanillaList = VersionDic["quiltVanilla"].FindAll(x => (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas));
                            cb_instances_mcmod_version.DataContext = localVanillaList.Select(x => x.Id);
                            cb_instances_mcmod_version.SelectedIndex = 0;

                            localModList = VersionDic[versionType];
                            cb_instances_mod_version.DataContext = localModList.Select(x => x.Id);
                            cb_instances_mod_version.SelectedIndex = 0;
                            return;
                        }
                }

                localVanillaList = VersionDic["forgeVanilla"].FindAll(x => (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas));
                cb_instances_mcmod_version.DataContext = localVanillaList.Select(x => x.Id);
                cb_instances_mcmod_version.SelectedIndex = 0;

                localModList = VersionDic[versionType].FindAll(x => x.VanillaId == localVanillaList[0].Id && (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas));
                cb_instances_mod_version.DataContext = localModList.Select(x => x.Id);
                cb_instances_mod_version.SelectedIndex = localModList.FindIndex(x => x.VanillaId == localVanillaList[0].Id);
            }
        }

        private void UpdateLaunchStatusBar(bool isVisible)
        {
            bo_launch_progress.IsEnabled = isVisible;
            bo_launch_progress.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
        }

        private void UpdateLaunchStatusBar(double value, string content)
        {
            lab_launch_progress.Content = content;
            pb_launch_progress.Value = value;
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

            // TODO - add progressbar updates, fix library download percent bug (visual)
            string nativesDir = string.Empty;
            string gameDir = string.Empty;
            string clientPath = string.Empty;
            string versionName = string.Empty;
            string libraryBundle = string.Empty;
            string clientId = "0"; // todo
            string xUID = "0"; // todo
            string argMainClass = string.Empty;
            string versionDir = string.Empty;

            UpdateLaunchStatusBar(true);
            UpdateLaunchStatusBar(0, $"Reading the vanillaManifest file...");
            List<string> argumnets = new List<string>();
            string gameExtraArgumnets = string.Empty;
            string jvmExtraArgumnets = string.Empty;

            // Read the Version Manifest
            VersionManifest? vanillaManifest = await JsonHelper.ReadJsonFileAsync<VersionManifest>(Path.Combine(IOHelper.ManifestDir, "vanillaManifest.json"));
            if (vanillaManifest == null)
            {
                NotificationHelper.SendError("Failed to get the vanilla vanillaManifest file.", "Error");
                return;
            }

            UpdateLaunchStatusBar(0, $"Checking the version directory and files...");

            // Check the profile type
            // GameManager.GetProfileVersionDetails(selectedProfile.Type, vanillaManifest, selectedProfile);
            VersionResponse vanillaVersion = new VersionResponse();
            switch (selectedProfile.Type)
            {
                case EProfileType.CUSTOM:
                    {
                        vanillaVersion = GameManager.GetProfileVersionDetails(EProfileKind.VANILLA, selectedProfile.VersionVanillaId, selectedProfile.VersionVanillaId, null);
                        break;
                    }
                default:
                    {
                        vanillaVersion = GameManager.GetProfileVersionDetails(selectedProfile.Type, vanillaManifest, selectedProfile);
                        break;
                    }
            }

            string nativeDir = Path.Combine(vanillaVersion.VersionDirectory, "natives");
            versionDir = vanillaVersion.VersionDirectory;
            versionName = vanillaVersion.InstanceVersion;
            List<MCLibrary> minecraftLibraries = new List<MCLibrary>();
            List<MCLibrary> nativeLibraries = new List<MCLibrary>();

            #region Vanilla
            // Find the right vanilla instanceVersion
            MCVersion? mcVersion = vanillaManifest.Versions.Find(x => x.Id == vanillaVersion.VanillaVersion);
            if (mcVersion == null)
            {
                NotificationHelper.SendError("Failed to get the minecraft version from the vanillaManifest file.", "Error");
                return;
            }

            #region Check vanilla manifest and get libraries and assets
            // Create versionDir in the versions folder
            if (!Directory.Exists(vanillaVersion.VersionDirectory))
                Directory.CreateDirectory(vanillaVersion.VersionDirectory);

            #region Check version json
            UpdateLaunchStatusBar(0, $"Checking the '{mcVersion.Id}' version json file...");
            MCVersionMeta? vanillaVersionMeta = null;
            if (!File.Exists(vanillaVersion.VersionJsonPath))
            {
                UpdateLaunchStatusBar(0, $"Downloading the '{mcVersion.Id}' version json file...");
                using (HttpClient client = new HttpClient())
                {
                    string jsonResult = await client.GetStringAsync(mcVersion.Url);

                    vanillaVersionMeta = JsonConvert.DeserializeObject<MCVersionMeta>(jsonResult);
                    if (vanillaVersionMeta != null)
                        foreach (var lib in vanillaVersionMeta.Libraries)
                            minecraftLibraries.Add(lib);

                    await File.WriteAllTextAsync(vanillaVersion.VersionJsonPath, jsonResult);
                }
            }
            else
            {
                UpdateLaunchStatusBar(0, $"Reading the '{mcVersion.Id}' version json file...");
                string jsonResult = await File.ReadAllTextAsync(vanillaVersion.VersionJsonPath);
                vanillaVersionMeta = JsonConvert.DeserializeObject<MCVersionMeta>(jsonResult);
                if (vanillaVersionMeta != null)
                    foreach (var lib in vanillaVersionMeta.Libraries)
                        minecraftLibraries.Add(lib);
            }

            if (vanillaVersionMeta == null)
            {
                NotificationHelper.SendError("Failed to get the vanilla version meta json file", "Error");
                return;
            }
            #endregion

            #region Download Vanilla Jar
            UpdateLaunchStatusBar(0, $"Checking the '{mcVersion.Id}' version jar file...");
            if (!File.Exists(vanillaVersion.VersionJarPath))
            {
                UpdateLaunchStatusBar(0, $"Downloading the '{mcVersion.Id}' version jar file...");
                using (HttpClient client = new HttpClient())
                {
                    byte[] bytes = await client.GetByteArrayAsync(vanillaVersionMeta.Downloads.Client.Url);
                    await File.WriteAllBytesAsync(vanillaVersion.VersionJarPath, bytes);
                }
            }
            #endregion

            #region Download assetIndex
            UpdateLaunchStatusBar(0, $"Checking the '{vanillaVersionMeta.AssetIndex.Id}' assetIndex json file...");
            string assetIndex = vanillaVersionMeta.AssetIndex.Id;
            string assetPath = Path.Combine(IOHelper.AssetsDir, $"indexes/{vanillaVersionMeta.AssetIndex.Id}.json");
            JToken? assetJToken = null;
            if (!File.Exists(assetPath))
            {
                UpdateLaunchStatusBar(0, $"Downloading the '{vanillaVersionMeta.AssetIndex.Id}' assetIndex json file...");
                using (HttpClient client = new HttpClient())
                {
                    string resultJson = await client.GetStringAsync(vanillaVersionMeta.AssetIndex.Url);
                    assetJToken = JObject.Parse(resultJson)["objects"];
                    await File.WriteAllTextAsync(assetPath, resultJson);
                }
            }
            else
            {
                UpdateLaunchStatusBar(0, $"Reading the '{vanillaVersionMeta.AssetIndex.Id}' assetIndex json file...");
                string resultJson = await File.ReadAllTextAsync(assetPath);
                assetJToken = JObject.Parse(resultJson)["objects"];
            }


            if (assetJToken == null)
            {
                NotificationHelper.SendError("Failed to get the assetJToken", "Error");
                return;
            }
            #endregion

            // Check AssetIndex Objects
            string assetObjectDir = Path.Combine(IOHelper.AssetsDir, "objects");
            if (!Directory.Exists(assetObjectDir))
                Directory.CreateDirectory(assetObjectDir);

            #region Download assets
            using (HttpClient client = new HttpClient())
            {
                string hash = string.Empty;
                string objectDir = string.Empty;
                string objectPath = string.Empty;
                int downloadedAssetSize = 0;
                UpdateLaunchStatusBar(0, $"Downloading assets... 0%");
                foreach (JToken token in assetJToken.ToList())
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
                    double percent = (double)downloadedAssetSize / (double)vanillaVersionMeta.AssetIndex.TotalSize * 100d;
                    lab_launch_progress.Content = $"Downloading assets... {pb_launch_progress.Value:0.00}%";
                    UpdateLaunchStatusBar(percent, $"Downloading assets... {percent:0.00}%");
                }
            }
            #endregion
            #endregion
            #endregion

            #region Check Moded Content
            string libraryCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
            if (!Directory.Exists(libraryCacheDir))
                Directory.CreateDirectory(libraryCacheDir);

            nativeDir = Path.Combine(vanillaVersion.VersionDirectory, "natives");
            string librarySizeCacheFilePath = string.Empty;
            switch (selectedProfile.Type)
            {
                case EProfileType.KONKORD_CREATE:
                    {

                        break;
                    }
                case EProfileType.KONKORD_VANILLAPLUS:
                    {

                        break;
                    }
                case EProfileType.CUSTOM:
                    {
                        switch (selectedProfile.Kind)
                        {
                            case EProfileKind.FORGE:
                                {
                                    librarySizeCacheFilePath = Path.Combine(libraryCacheDir, $"{selectedProfile.VersionVanillaId}-forge-{selectedProfile.VersionId}.json");
                                    
                                    if (!File.Exists(IOHelper.ForgeManifestJsonFile))
                                    {
                                        NotificationHelper.SendError("Failed to get the forge manifest.", "Error");
                                        return;
                                    }

                                    VersionResponse forgeVersion = GameManager.GetProfileVersionDetails(EProfileKind.FORGE, selectedProfile.VersionId, selectedProfile.VersionVanillaId, selectedProfile.GameDirectory);

                                    // Create versionDir in the versions folder
                                    if (!Directory.Exists(forgeVersion.VersionDirectory))
                                        Directory.CreateDirectory(forgeVersion.VersionDirectory);

                                    // Download Forge Installer to temp
                                    string forgeUniversalJarUrl = string.Format(GameManager.ForgeLoaderUniversalJarUrl, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}");
                                    string forgeInstallerJarUrl = string.Format(GameManager.ForgeInstallerJarUrl, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}");

                                    JObject forgeInstallProfileObj = new JObject();
                                    string forgeInstallProfileJsonPath = Path.Combine(forgeVersion.VersionDirectory, "install_profile.json");

                                    // Get Cache File Path
                                    string librarySizeCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
                                    string librarySizeCachePath = Path.Combine(librarySizeCacheDir, $"{forgeVersion.VanillaVersion}-forge-{forgeVersion.InstanceVersion}.json");

                                    string forgeInstallerFilePath = Path.Combine(IOHelper.TempDir, $"{forgeVersion.VanillaVersion}-forge-{forgeVersion.InstanceVersion}-installer.jar");
                                    string forgeInstallerDirPath = Path.Combine(IOHelper.TempDir, $"{forgeVersion.VanillaVersion}-forge-{forgeVersion.InstanceVersion}-installer");
                                    string forgeInstallerVersionPath = Path.Combine(forgeInstallerDirPath, $"version.json");
                                    string forgeInstallerProfilePath = Path.Combine(forgeInstallerDirPath, $"install_profile.json");

                                    // Check the version json file
                                    if (!File.Exists(forgeVersion.VersionJsonPath))
                                    {
                                        using (var client = new HttpClient())
                                        {
                                            // Send the request to download the installer
                                            byte[] installerBytes = await client.GetByteArrayAsync(forgeInstallerJarUrl);
                                            await File.WriteAllBytesAsync(forgeInstallerFilePath, installerBytes);

                                            // Extract installer jar
                                            ZipFile.ExtractToDirectory(forgeInstallerFilePath, forgeInstallerDirPath);

                                            // Move version.json
                                            File.Move(forgeInstallerVersionPath, forgeVersion.VersionJsonPath, true);
                                            // Move install_profile.json
                                            File.Move(forgeInstallerProfilePath, forgeInstallProfileJsonPath, true);

                                            forgeInstallProfileObj = JObject.Parse(await File.ReadAllTextAsync(forgeInstallProfileJsonPath));

                                            // Get jobject
                                            ForgeVersionMeta? forgeVersionMeta = JsonConvert.DeserializeObject<ForgeVersionMeta>(await File.ReadAllTextAsync(forgeVersion.VersionJsonPath));
                                            int localLibrarySize = 0;
                                            if (forgeVersionMeta== null)
                                            {
                                                NotificationHelper.SendError("Failed to get the forge version meta file.", "Error");
                                                return;
                                            }

                                            // Check the libraries
                                            foreach (MCLibrary lib in forgeVersionMeta.Libraries)
                                            {
                                                localLibrarySize += lib.Downloads.Artifact.Size;
                                                minecraftLibraries.Add(lib);
                                            }
                                            // Save the version cache
                                            await JsonHelper.WriteJsonFileAsync(librarySizeCachePath, localLibrarySize);


                                            argMainClass = forgeVersionMeta.MainClass;
                                            gameExtraArgumnets += forgeVersionMeta.Arguments.GetGameArgString();
                                            jvmExtraArgumnets += forgeVersionMeta.Arguments.GetJVMArgString();
                                        }
                                    }
                                    else
                                    {
                                        // Get jobject
                                        ForgeVersionMeta? forgeVersionMeta = JsonConvert.DeserializeObject<ForgeVersionMeta>(await File.ReadAllTextAsync(forgeVersion.VersionJsonPath));
                                        int localLibrarySize = 0;
                                        if (forgeVersionMeta == null)
                                        {
                                            NotificationHelper.SendError("Failed to get the forge version meta file.", "Error");
                                            return;
                                        }

                                        // Check the libraries
                                        foreach (MCLibrary lib in forgeVersionMeta.Libraries)
                                        {
                                            minecraftLibraries.Add(lib);
                                        }

                                        forgeInstallProfileObj = JObject.Parse(await File.ReadAllTextAsync(forgeInstallProfileJsonPath));

                                        argMainClass = forgeVersionMeta.MainClass;
                                        gameExtraArgumnets += forgeVersionMeta.Arguments.GetGameArgString();
                                        jvmExtraArgumnets += forgeVersionMeta.Arguments.GetJVMArgString() + 
                                            $" -DMcEmu=net.minecraft.client.main.Main -Dlog4j2.formatMsgNoLookups=true -Djava.rmi.server.useCodebaseOnly=true -Dcom.sun.jndi.rmi.object.trustURLCodebase=false -Dcom.sun.jndi.cosnaming.object.trustURLCodebase=false ";
                                    }

                                    // Delete temps
                                    if (Directory.Exists(forgeInstallerDirPath))
                                        IOHelper.DeleteDirectory(forgeInstallerDirPath);
                                    if (File.Exists(forgeInstallerFilePath))
                                        File.Delete(forgeInstallerFilePath);

                                    // Create gameDir in the instances folder
                                    if (!Directory.Exists(forgeVersion.GameDir))
                                        Directory.CreateDirectory(forgeVersion.GameDir);

                                    versionName = $"{forgeVersion.VanillaVersion}-forge-{forgeVersion.InstanceVersion}";
                                    versionDir = forgeVersion.VersionDirectory;
                                    gameDir = forgeVersion.GameDir;
                                    nativeDir = Path.Combine(forgeVersion.VersionDirectory, "natives");

                                    if (!File.Exists(forgeVersion.VersionJarPath))
                                    {
                                        File.Copy(forgeVersion.VanillaJarPath, forgeVersion.VersionJarPath);
                                    }

                                    clientPath = forgeVersion.VersionJarPath;
                                    break;
                                }
                            case EProfileKind.FABRIC:
                                {
                                    librarySizeCacheFilePath = Path.Combine(libraryCacheDir, $"{selectedProfile.VersionVanillaId}-fabric-{selectedProfile.VersionId}.json");

                                    if (!File.Exists(IOHelper.FabricManifestJsonFile))
                                    {
                                        NotificationHelper.SendError("Failed to get the fabric manifest.", "Error");
                                        return;
                                    }

                                    VersionResponse fabricVersion = GameManager.GetProfileVersionDetails(EProfileKind.FABRIC, selectedProfile.VersionId, selectedProfile.VersionVanillaId, selectedProfile.GameDirectory);

                                    // Create versionDir in the versions folder
                                    if (!Directory.Exists(fabricVersion.VersionDirectory))
                                        Directory.CreateDirectory(fabricVersion.VersionDirectory);

                                    // Check libsizes dir
                                    string librarySizeCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
                                    if (!Directory.Exists(librarySizeCacheDir))
                                        Directory.CreateDirectory(librarySizeCacheDir);
                                    string librarySizeCachePath = Path.Combine(librarySizeCacheDir, $"{fabricVersion.VanillaVersion}-fabric-{fabricVersion.InstanceVersion}.json");

                                    // Download version json
                                    FabricVersionMeta? fabricVersionMeta = null;
                                    List<MCLibrary> localLibraries = new List<MCLibrary>();
                                    if (!File.Exists(fabricVersion.VersionJsonPath))
                                    {
                                        string resultJson = string.Empty;
                                        using (HttpClient client = new HttpClient())
                                        {
                                            resultJson = await client.GetStringAsync(string.Format(GameManager.FabricLoaderJsonUrl, fabricVersion.VanillaVersion, fabricVersion.InstanceVersion));
                                            await File.WriteAllTextAsync(fabricVersion.VersionJsonPath, resultJson);
                                        }

                                        // Add the libraries
                                        fabricVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(resultJson);
                                        int localLibrarySize = 0;
                                        if (fabricVersionMeta == null)
                                        {
                                            File.Delete(fabricVersion.VersionJsonPath); // Delete it because this if part won't be executed again if it exists
                                            NotificationHelper.SendError("Failed to get the fabric version meta", "Error");
                                            return;
                                        }

                                        foreach (var lib in fabricVersionMeta.Libraries)
                                        {
                                            localLibrarySize += lib.Size;
                                            localLibraries.Add(new MCLibrary(lib.Name, new MCLibraryDownloads(new MCLibraryArtifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL())), new List<MCLibraryRule>()));
                                        }
                                        // Save the version cache
                                        await JsonHelper.WriteJsonFileAsync(librarySizeCachePath, localLibrarySize);
                                    }
                                    else
                                    {
                                        fabricVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(await File.ReadAllTextAsync(fabricVersion.VersionJsonPath));
                                        if (fabricVersionMeta == null)
                                        {
                                            NotificationHelper.SendError("Failed to get the fabric version meta", "Error");
                                            return;
                                        }

                                        foreach (var lib in fabricVersionMeta.Libraries)
                                        {
                                            localLibraries.Add(new MCLibrary(lib.Name, new MCLibraryDownloads(new MCLibraryArtifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL())), new List<MCLibraryRule>()));
                                        }
                                    }
                                    // Include libraries
                                    minecraftLibraries.InsertRange(0, localLibraries);

                                    // Download Loader
                                    string loaderDirPath = Path.Combine(IOHelper.LibrariesDir, $"net\\fabricmc\\fabric-loader\\{fabricVersion.InstanceVersion}");
                                    string loaderJarPath = Path.Combine(loaderDirPath, $"fabric-loader-{fabricVersion.InstanceVersion}.jar");
                                    if (!Directory.Exists(loaderDirPath))
                                        Directory.CreateDirectory(loaderDirPath);

                                    if (!File.Exists(loaderJarPath))
                                    {
                                        using (HttpClient client = new HttpClient())
                                        {
                                            byte[] bytes = await client.GetByteArrayAsync(string.Format(GameManager.FabricLoaderJarUrl, fabricVersion.InstanceVersion));
                                            await File.WriteAllBytesAsync(loaderJarPath, bytes);
                                        }
                                    }

                                    versionName = $"fabric-loader-{fabricVersion.InstanceVersion}-{fabricVersion.VanillaVersion}";
                                    nativeDir = Path.Combine(fabricVersion.VersionDirectory, "natives");
                                    versionDir = fabricVersion.VersionDirectory;
                                    gameDir = fabricVersion.GameDir;
                                    argMainClass = fabricVersionMeta.MainClass;
                                    gameExtraArgumnets += $"" + fabricVersionMeta.Arguments.GetGameArgString();
                                    jvmExtraArgumnets += fabricVersionMeta.Arguments.GetJVMArgString().Replace("-DFabricMcEmu= net.minecraft.client.main.Main ", "\"-DFabricMcEmu= net.minecraft.client.main.Main \"") +
                                        $" -DMcEmu=net.minecraft.client.main.Main -Dlog4j2.formatMsgNoLookups=true -Djava.rmi.server.useCodebaseOnly=true -Dcom.sun.jndi.rmi.object.trustURLCodebase=false ";

                                    if (!File.Exists(fabricVersion.VersionJarPath))
                                    {
                                        File.Copy(fabricVersion.VanillaJarPath, fabricVersion.VersionJarPath);
                                    }

                                    clientPath = fabricVersion.VersionJarPath;
                                    break;
                                }
                            case EProfileKind.QUILT:
                                {
                                    librarySizeCacheFilePath = Path.Combine(libraryCacheDir, $"{selectedProfile.VersionVanillaId}-quilt-{selectedProfile.VersionId}.json");
                                    if (!File.Exists(IOHelper.QuiltManifestJsonFile))
                                    {
                                        NotificationHelper.SendError("Failed to get the quilt manifest.", "Error");
                                        return;
                                    }

                                    VersionResponse quiltVersion = GameManager.GetProfileVersionDetails(EProfileKind.QUILT, selectedProfile.VersionId, selectedProfile.VersionVanillaId, selectedProfile.GameDirectory);

                                    // Create versionDir in the versions folder
                                    if (!Directory.Exists(quiltVersion.VersionDirectory))
                                        Directory.CreateDirectory(quiltVersion.VersionDirectory);

                                    // Check libsizes dir
                                    string librarySizeCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
                                    if (!Directory.Exists(librarySizeCacheDir))
                                        Directory.CreateDirectory(librarySizeCacheDir);
                                    string librarySizeCachePath = Path.Combine(librarySizeCacheDir, $"{quiltVersion.VanillaVersion}-quilt-{quiltVersion.InstanceVersion}.json");

                                    // Download version json
                                    FabricVersionMeta? quiltVersionMeta = null;
                                    List<MCLibrary> localLibraries = new List<MCLibrary>();
                                    if (!File.Exists(quiltVersion.VersionJsonPath))
                                    {
                                        string resultJson = string.Empty;
                                        using (HttpClient client = new HttpClient())
                                        {
                                            resultJson = await client.GetStringAsync(string.Format(GameManager.QuiltLoaderJsonUrl, quiltVersion.VanillaVersion, quiltVersion.InstanceVersion));
                                            await File.WriteAllTextAsync(quiltVersion.VersionJsonPath, resultJson);
                                        }

                                        // Add the libraries
                                        quiltVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(resultJson);
                                        int localLibrarySize = 0;
                                        if (quiltVersionMeta == null)
                                        {
                                            File.Delete(quiltVersion.VersionJsonPath); // Delete it because this if part won't be executed again if it exists
                                            NotificationHelper.SendError("Failed to get the quilt version meta", "Error");
                                            return;
                                        }

                                        foreach (var lib in quiltVersionMeta.Libraries)
                                        {
                                            localLibrarySize += lib.Size;
                                            localLibraries.Add(new MCLibrary(lib.Name, new MCLibraryDownloads(new MCLibraryArtifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL())), new List<MCLibraryRule>()));
                                        }
                                        // Save the version cache
                                        await JsonHelper.WriteJsonFileAsync(librarySizeCachePath, localLibrarySize);
                                    }
                                    else
                                    {
                                        quiltVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(await File.ReadAllTextAsync(quiltVersion.VersionJsonPath));
                                        if (quiltVersionMeta == null)
                                        {
                                            NotificationHelper.SendError("Failed to get the quilt version meta", "Error");
                                            return;
                                        }

                                        foreach (var lib in quiltVersionMeta.Libraries)
                                        {
                                            localLibraries.Add(new MCLibrary(lib.Name, new MCLibraryDownloads(new MCLibraryArtifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL())), new List<MCLibraryRule>()));
                                        }
                                    }
                                    // Include libraries
                                    minecraftLibraries.InsertRange(0, localLibraries);

                                    // Download Loader
                                    string loaderDirPath = Path.Combine(IOHelper.LibrariesDir, $"net\\fabricmc\\quilt-loader\\{quiltVersion.InstanceVersion}");
                                    string loaderJarPath = Path.Combine(loaderDirPath, $"fabricmc-loader-{quiltVersion.InstanceVersion}.jar");
                                    if (!Directory.Exists(loaderDirPath))
                                        Directory.CreateDirectory(loaderDirPath);

                                    if (!File.Exists(loaderJarPath))
                                    {
                                        using (HttpClient client = new HttpClient())
                                        {
                                            byte[] bytes = await client.GetByteArrayAsync(string.Format(GameManager.QuiltLoaderJarUrl, quiltVersion.InstanceVersion));
                                            await File.WriteAllBytesAsync(loaderJarPath, bytes);
                                        }
                                    }

                                    versionName = $"quilt-loader-{quiltVersion.InstanceVersion}-{quiltVersion.VanillaVersion}";
                                    nativeDir = Path.Combine(quiltVersion.VersionDirectory, "natives");
                                    versionDir = quiltVersion.VersionDirectory;
                                    gameDir = quiltVersion.GameDir;
                                    argMainClass = quiltVersionMeta.MainClass;
                                    gameExtraArgumnets += $"" + quiltVersionMeta.Arguments.GetGameArgString();
                                    jvmExtraArgumnets += quiltVersionMeta.Arguments.GetJVMArgString() +
                                        $" -DMcEmu=net.minecraft.client.main.Main -Dlog4j2.formatMsgNoLookups=true -Djava.rmi.server.useCodebaseOnly=true -Dcom.sun.jndi.rmi.object.trustURLCodebase=false ";

                                    if (!File.Exists(quiltVersion.VersionJarPath))
                                    {
                                        File.Copy(quiltVersion.VanillaJarPath, quiltVersion.VersionJarPath);
                                    }

                                    clientPath = quiltVersion.VersionJarPath;
                                    break;
                                }
                            default:
                                {
                                    // Create gameDir in the instances folder
                                    if (!Directory.Exists(vanillaVersion.GameDir))
                                        Directory.CreateDirectory(vanillaVersion.GameDir);

                                    gameDir = vanillaVersion.GameDir;
                                    librarySizeCacheFilePath = Path.Combine(libraryCacheDir, $"{selectedProfile.VersionId}.json");
                                    argMainClass = vanillaVersionMeta.MainClass;

                                    clientPath = vanillaVersion.VersionJarPath;
                                    break;
                                }

                        }
                        break;
                    }
                case EProfileType.LATEST_RELEASE:
                    {
                        // Create gameDir in the instances folder
                        if (!Directory.Exists(vanillaVersion.GameDir))
                            Directory.CreateDirectory(vanillaVersion.GameDir);

                        gameDir = vanillaVersion.GameDir;
                        librarySizeCacheFilePath = Path.Combine(libraryCacheDir, $"{vanillaManifest.Latest.Release}.json");
                        argMainClass = vanillaVersionMeta.MainClass;

                        clientPath = vanillaVersion.VersionJarPath;
                        break;
                    }
                case EProfileType.LATEST_SNAPSHOT:
                    {
                        // Create gameDir in the instances folder
                        if (!Directory.Exists(vanillaVersion.GameDir))
                            Directory.CreateDirectory(vanillaVersion.GameDir);

                        gameDir = vanillaVersion.GameDir;
                        librarySizeCacheFilePath = Path.Combine(libraryCacheDir, $"{vanillaManifest.Latest.Snapshot}.json");
                        argMainClass = vanillaVersionMeta.MainClass;

                        clientPath = vanillaVersion.VersionJarPath;
                        break;
                    }
            }
            #endregion

            #region Client Mappings
            string clientMappinsPath = Path.Combine(versionDir, "client.txt");
            if (!File.Exists(clientMappinsPath))
            {
                using (HttpClient client = new HttpClient())
                {
                    string r = await client.GetStringAsync(vanillaVersionMeta.Downloads.ClientMappings.Url);
                    await File.WriteAllTextAsync(clientMappinsPath, r);
                }
            }
            #endregion

            #region Check logging file
            if (vanillaVersionMeta.Logging != null && vanillaVersionMeta.Logging.Client != null)
            {
                string logFilePath = Path.Combine(versionDir, vanillaVersionMeta.Logging.Client.File.Id);
                if (!File.Exists(logFilePath))
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string r = await client.GetStringAsync(vanillaVersionMeta.Logging.Client.File.Url);
                        await File.WriteAllTextAsync(logFilePath, r);
                    }
                }
                jvmExtraArgumnets += vanillaVersionMeta.Logging.Client.Argument.Replace("${path}", logFilePath);
            }
            #endregion

            #region Download Libraries
            UpdateLaunchStatusBar(0, $"Checking the libraries...");
            double libraryOverallSize = 0;
            double libraryDownloadedSize = 0;
            
            using (HttpClient client = new HttpClient())
            {
                // Calculate the overallSize or read it from cache
                if (!File.Exists(librarySizeCacheFilePath))
                {
                    foreach (var lib in minecraftLibraries)
                        libraryOverallSize += lib.Downloads.Artifact.Size;

                    await File.WriteAllTextAsync(librarySizeCacheFilePath, libraryOverallSize.ToString());
                }
                else
                    libraryOverallSize = int.Parse(await File.ReadAllTextAsync(librarySizeCacheFilePath));

                // Download the actual libs
                foreach (var lib in minecraftLibraries)
                {
                    // Check the library rule
                    if (lib.Rules != null)
                        if (lib.Rules.Count > 0)
                        {
                            bool action = lib.Rules[0].Action == "allow";
                            if (lib.Rules[0].OS != null)
                            {
                                switch (lib.Rules[0].OS.Name)
                                {
                                    case "osx": // lib requies machintosh
                                        {
                                            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && action)
                                            {
                                                libraryDownloadedSize += lib.Downloads.Artifact.Size;
                                                continue;
                                            }
                                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !action)
                                            {
                                                libraryDownloadedSize += lib.Downloads.Artifact.Size;
                                                continue;
                                            }
                                            break;
                                        }
                                    case "linux":
                                        {
                                            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && action)
                                            {
                                                libraryDownloadedSize += lib.Downloads.Artifact.Size;
                                                continue;
                                            }
                                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !action)
                                            {
                                                libraryDownloadedSize += lib.Downloads.Artifact.Size;
                                                continue;
                                            }
                                            break;
                                        }
                                    case "windows":
                                        {
                                            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && action)
                                            {
                                                libraryDownloadedSize += lib.Downloads.Artifact.Size;
                                                continue;
                                            }
                                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !action)
                                            {
                                                libraryDownloadedSize += lib.Downloads.Artifact.Size;
                                                continue;
                                            }
                                            break;
                                        }
                                }
                            }
                        }

                    string localPath = lib.Downloads.Artifact.Path;
                    string libDirPath = Path.Combine(IOHelper.LibrariesDir, localPath.Remove(localPath.LastIndexOf('/'), localPath.Length - localPath.LastIndexOf('/')));
                    if (!Directory.Exists(libDirPath))
                        Directory.CreateDirectory(libDirPath);
                    
                    string libFilePath = Path.Combine(IOHelper.LibrariesDir, localPath);
                    if (!File.Exists(libFilePath))
                    {
                        if (!string.IsNullOrEmpty(lib.Downloads.Artifact.Url))
                        {
                            byte[] bytes = await client.GetByteArrayAsync(lib.Downloads.Artifact.Url);
                            await File.WriteAllBytesAsync(libFilePath, bytes);
                        }
                    }

                    if (!libraryBundle.Contains(libFilePath))
                    {
                        libraryBundle += $"{libFilePath};";
                        if (lib.Name.StartsWith("org.lwjgl"))
                        {
                            nativeLibraries.Add(lib);
                        }
                    }
                    libraryDownloadedSize += lib.Downloads.Artifact.Size;
                    if (libraryOverallSize == 0)
                        libraryOverallSize = 1; // NaN fix
                    double percent = libraryDownloadedSize / libraryOverallSize * 100;
                    UpdateLaunchStatusBar(percent, $"Downloading the '{lib.Name}' library... {percent:0.00}%");
                }
            }
            #endregion

            libraryBundle += $"{clientPath}";
            Debug.WriteLine($"#5 -> Added ClassPath: {clientPath}");
            UpdateLaunchStatusBar(0, $"Launching the game...");

            // Create natives directory
            if (!Directory.Exists(nativeDir))
                Directory.CreateDirectory(nativeDir);

            // Extract natives
            foreach (var lib in nativeLibraries)
            {
                string localPath = lib.Downloads.Artifact.Path;
                string libFilePath = Path.Combine(IOHelper.LibrariesDir, localPath);

                string tempZipDir = Path.Combine(IOHelper.TempDir, localPath.Split('.')[0]);

                ZipFile.ExtractToDirectory(libFilePath, tempZipDir);

                string[] files = Directory.GetFiles(tempZipDir, "*.dll", searchOption: SearchOption.AllDirectories);
                if (files != null)
                {
                    foreach (string file in files)
                    {
                        string fileName = file.Remove(0, file.LastIndexOf('\\') + 1);
                        string filePath = Path.Combine(nativeDir, fileName);
                        if (!File.Exists(filePath))
                            File.Move(file, filePath);
                    }
                }

                IOHelper.DeleteDirectory(tempZipDir);
            }

            #region Arguments
            #region JVM
            argumnets.Add("-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump");
            // Vanilla Args
            argumnets.Add(vanillaVersionMeta.Arguments.GetJVMArgString());

            // Moded Args
            if (!string.IsNullOrEmpty(jvmExtraArgumnets))
                argumnets.Add(jvmExtraArgumnets);

            // Maximum Memory
            if (selectedProfile.Memory > 0 && selectedProfile.Memory <= 256)
                argumnets.Add($"-Xmx{selectedProfile.Memory}G");
            else if (selectedProfile.Memory > 256)
                argumnets.Add($"-Xmx{selectedProfile.Memory}M");
            else
                argumnets.Add($"-Xmx4G");
            // Minimum Memory
            // If someone breaks it by setting the maximum memory lower than 256MB... do not hit them
            argumnets.Add($"-Xms256M");

            // The JVM args set by the user
            if (!string.IsNullOrEmpty(selectedProfile.JVMArgs))
                argumnets.Add($"{selectedProfile.JVMArgs}");
            #endregion
            #region Minecraft Args
            // The main class
            argumnets.Add(argMainClass);
            // Vanilla Args
            argumnets.Add(vanillaVersionMeta.Arguments.GetGameArgString());
            // Moded Args
            if (!string.IsNullOrEmpty(gameExtraArgumnets))
                argumnets.Add(gameExtraArgumnets);
            #endregion

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

            string argumentString = string.Join(' ', argumnets);
            #region Replace Variable Placeholders
            argumentString = argumentString
                //.Replace("-Djava.library.path=${natives_directory}", "")
                .Replace("${natives_directory}", nativeDir)
                .Replace("${launcher_name}", "konkord-launcher")
                .Replace("${launcher_version}", "release")
                .Replace("${auth_player_name}", account.DisplayName)
                .Replace("${version_name}", versionName)
                .Replace("${game_directory}", gameDir)
                .Replace("${assets_root}", IOHelper.AssetsDir)
                .Replace("${assets_index_name}", assetIndex)
                .Replace("${auth_uuid}", account.UUID)
                .Replace("${auth_access_token}", string.IsNullOrEmpty(account.AccessToken) ? "none" : account.AccessToken)
                .Replace("${clientid}", clientId)
                .Replace("${auth_xuid}", xUID)
                .Replace("${user_type}", "msa")
                .Replace("${version_type}", "release")
                .Replace("${classpath}", $"\"{libraryBundle}\"")
                .Replace("${library_directory}", IOHelper.LibrariesDir);
            #endregion
            #endregion

            #region Finish and Launch Minecraft
            // Enable play button
            bo_launch_play.IsEnabled = true;
            btn_launch_play.IsEnabled = true;
            // Disable progressbar
            UpdateLaunchStatusBar(false);
            //Launch game instance

            var psi = new ProcessStartInfo()
            {
                FileName = $"{selectedProfile.JavaPath}java",
                Arguments = argumentString,
                UseShellExecute = false,
                RedirectStandardError = true,
            };
            Process? p = Process.Start(psi);
            await p.WaitForExitAsync();
            string o = await p.StandardError.ReadToEndAsync();
            if (!string.IsNullOrEmpty(o))
                NotificationHelper.SendError(o, "Error");

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
        public string InstanceMCVersionId { get; set; }
        public string InstanceModVersionId {  get; set; }
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

            if ((cb_instances_mc_version.IsEnabled && cb_instances_mc_version.SelectedIndex < 0) || (cb_instances_mcmod_version.IsEnabled && (cb_instances_mcmod_version.SelectedIndex < 0 || cb_instances_mod_version.SelectedIndex < 0)))
            {
                NotificationHelper.SendError("You must select a version.", "Error");
                return;
            }
            EProfileKind profileKind;
            switch (cb_instances_version_type.SelectedIndex)
            {
                default:
                case 0:
                    {
                        profileKind = EProfileKind.VANILLA;
                        break;
                    }
                case 1:
                    {
                        profileKind = EProfileKind.FORGE;
                        break;
                    }
                case 2:
                    {
                        profileKind = EProfileKind.FABRIC;
                        break;
                    }
                case 3:
                    {
                        profileKind = EProfileKind.FORGE;
                        break;
                    }
                case 4:
                    {
                        profileKind = EProfileKind.QUILT;
                        break;
                    }
            }
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
                    VersionId = InstanceModVersionId,
                    VersionVanillaId = InstanceMCVersionId, 
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
        /// Handles the selection changed event of the instance instanceVersion type ComboBox.
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

                        cb_instances_mc_version.IsEnabled = true;
                        cb_instances_mcmod_version.IsEnabled = false;
                        cb_instances_mod_version.IsEnabled = false;
                        cb_instances_mc_version.Visibility = Visibility.Visible;
                        cb_instances_mcmod_version.Visibility = Visibility.Hidden;
                        cb_instances_mod_version.Visibility = Visibility.Hidden;
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

                        cb_instances_mc_version.IsEnabled = false;
                        cb_instances_mcmod_version.IsEnabled = true;
                        cb_instances_mod_version.IsEnabled = true;
                        cb_instances_mc_version.Visibility = Visibility.Hidden;
                        cb_instances_mcmod_version.Visibility = Visibility.Visible;
                        cb_instances_mod_version.Visibility = Visibility.Visible;
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

                        cb_instances_mc_version.IsEnabled = false;
                        cb_instances_mcmod_version.IsEnabled = true;
                        cb_instances_mod_version.IsEnabled = true;
                        cb_instances_mc_version.Visibility = Visibility.Hidden;
                        cb_instances_mcmod_version.Visibility = Visibility.Visible;
                        cb_instances_mod_version.Visibility = Visibility.Visible;
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

                        cb_instances_mc_version.IsEnabled = false;
                        cb_instances_mcmod_version.IsEnabled = true;
                        cb_instances_mod_version.IsEnabled = true;
                        cb_instances_mc_version.Visibility = Visibility.Hidden;
                        cb_instances_mcmod_version.Visibility = Visibility.Visible;
                        cb_instances_mod_version.Visibility = Visibility.Visible;
                        break;
                    }
            }
            RefreshDropdownVersions(version);
        }

        /// <summary>
        /// Handles the checked event of the instances instanceVersion CheckBox.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void InstancesVersion_Checked(object sender, RoutedEventArgs e)
        {
            RefreshDropdownVersions(cb_instances_version_type.SelectedValue.ToString().ToLower());
        }

        private void InstancesMcmodVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (checkb_instances_version_betas == null || checkb_instances_version_releases == null || checkb_instances_version_snapshots == null)
                return;

            bool showReleases = true;
            if (checkb_instances_version_releases.IsEnabled)
                showReleases = checkb_instances_version_releases.IsChecked.Value;
            bool showSnapshots = false;
            if (checkb_instances_version_snapshots.IsEnabled)
                showSnapshots = checkb_instances_version_snapshots.IsChecked.Value;
            bool showOldBetas = false;
            if (checkb_instances_version_betas.IsEnabled)
                showOldBetas = checkb_instances_version_betas.IsChecked.Value;


            List<VersionBase> localVanillaList = new List<VersionBase>();
            List<VersionBase> localModList = new List<VersionBase>();
            string versionType = cb_instances_version_type.Text.ToLower();
            if (versionType == "neoforge")
                versionType = "forge";

            ComboBox combo;
            switch (versionType)
            {
                case "fabric":
                    {
                        localVanillaList = VersionDic["fabricVanilla"].FindAll(x => (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas));

                        localModList = VersionDic[versionType];
                        cb_instances_mod_version.DataContext = localModList.Select(x => x.Id);
                        cb_instances_mod_version.SelectedIndex = 0;

                        if (cb_instances_mcmod_version == null)
                            return;

                        if (!cb_instances_mcmod_version.IsEnabled)
                            return;

                        combo = (ComboBox)sender;

                        if (combo.SelectedItem == null)
                            return;
                        InstanceMCVersionId = combo.SelectedItem.ToString();
                        Debug.WriteLine($"minecraft moded version: {InstanceMCVersionId}");
                        return;
                    }
                case "quilt":
                    {
                        localVanillaList = VersionDic["quiltVanilla"].FindAll(x => (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas));

                        localModList = VersionDic[versionType];
                        cb_instances_mod_version.DataContext = localModList.Select(x => x.Id);
                        cb_instances_mod_version.SelectedIndex = 0;

                        if (cb_instances_mcmod_version == null)
                            return;

                        if (!cb_instances_mcmod_version.IsEnabled)
                            return;

                        combo = (ComboBox)sender;

                        if (combo.SelectedItem == null)
                            return;
                        InstanceMCVersionId = combo.SelectedItem.ToString();
                        Debug.WriteLine($"minecraft moded version: {InstanceMCVersionId}");
                        return;
                    }
            }

            localVanillaList = VersionDic["forgeVanilla"].FindAll(x => (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas));

            localModList = VersionDic[versionType].FindAll(x => x.VanillaId == localVanillaList[cb_instances_mcmod_version.SelectedIndex].Id && (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas));
            cb_instances_mod_version.DataContext = localModList.Select(x => x.Id);
            cb_instances_mod_version.SelectedIndex = localModList.FindIndex(x => x.VanillaId == localVanillaList[cb_instances_mcmod_version.SelectedIndex].Id);

            if (cb_instances_mcmod_version == null)
                return;

            if (!cb_instances_mcmod_version.IsEnabled)
                return;

            combo = (ComboBox)sender;

            if (combo.SelectedItem == null)
                return;
            InstanceMCVersionId = combo.SelectedItem.ToString();
            Debug.WriteLine($"minecraft moded version: {InstanceMCVersionId}");
        }

        private void cb_instances_mc_version_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_instances_mc_version == null)
                return;

            if (!cb_instances_mc_version.IsEnabled)
                return;

            ComboBox combo = (ComboBox)sender;

            if (combo.SelectedItem == null)
                return;
            InstanceMCVersionId = combo.SelectedItem.ToString();
            InstanceModVersionId = combo.SelectedItem.ToString();
            Debug.WriteLine($"minecraft vanilla version: {InstanceMCVersionId}");
        }

        private void cb_instances_mod_version_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_instances_mod_version == null)
                return;

            if (!cb_instances_mod_version.IsEnabled)
                return;

            ComboBox combo = (ComboBox)sender;

            if (combo.SelectedItem == null)
                return;
            InstanceModVersionId = combo.SelectedItem.ToString();
            Debug.WriteLine($"mod version: {InstanceModVersionId}");
        }
        #endregion
    }
}

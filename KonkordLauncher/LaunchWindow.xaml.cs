using Tavstal.KonkordLibrary.Enums;
using Tavstal.KonkordLibrary.Helpers;
using Tavstal.KonkordLibrary.Models.Fabric;
using Tavstal.KonkordLibrary.Models.Installer;
using Tavstal.KonkordLibrary.Models.Minecraft;
using Tavstal.KonkordLibrary.Models.Quilt;
using Tavstal.KonkordLibrary.Models.Forge;
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
using Tavstal.KonkordLibrary.Managers;
using Tavstal.KonkordLibrary.Models.Launcher;
using KonkordLibrary.Models.Launcher;
using Tavstal.KonkordLibrary.Models.Minecraft.API;

namespace Tavstal.KonkordLauncher
{
    /// <summary>
    /// Interaction logic for LaunchWindow.xaml
    /// </summary>
    public partial class LaunchWindow : Window
    {
        // TODO - Patch the chaos

        private string? EditedProfileKey {  get; set; }
        private Profile? EditedProfile {  get; set; }
        private Grid SelectedSkinGrid {  get; set; }
        public KeyValuePair<string, Profile> SelectedProfile { get; set; }
        private readonly double _heightMultiplier;
        private readonly double _widthMultiplier;

        public LaunchWindow()
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
            Thickness localThickness;

            #region Main Window
            WindowHelper.Resize(bo_topmenu, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_topmenu_home, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_topmenu_modpacks, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_topmenu_patchs, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_topmenu_skins, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(grid_main, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(grid_main_home, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(grid_main_skins, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(grid_main_patchs, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(grid_main_modpacks, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(grid_main, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(bo_grid_main_skins_bg, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(bo_grid_main, _heightMultiplier, _widthMultiplier);

            #region Skins
            WindowHelper.ResizeFont(lab_main_skins_current, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_main_skins_library, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_main_skins_addtolibrary, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(bo_main_skins_addtolibrary, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_main_skins_newskin, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(bo_main_skins_newskin, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(bo_main_skins_divider, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(lb_main_skins, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(img_main_skins_current, _heightMultiplier, _widthMultiplier);

            lb_main_skins.Resources["ListButtonFontSize"] = double.Parse(lb_main_skins.Resources["ListButtonFontSize"].ToString()) * _widthMultiplier;
            lb_main_skins.Resources["GridWidth"] = double.Parse(lb_main_skins.Resources["GridWidth"].ToString()) * _widthMultiplier;
            lb_main_skins.Resources["GridHeight"] = double.Parse(lb_main_skins.Resources["GridHeight"].ToString()) * _heightMultiplier;
            lb_main_skins.Resources["LabelFontSize"] = double.Parse(lb_main_skins.Resources["LabelFontSize"].ToString()) * _widthMultiplier;
            lb_main_skins.Resources["ButtonHeight"] = double.Parse(lb_main_skins.Resources["ButtonHeight"].ToString()) * _heightMultiplier;
            lb_main_skins.Resources["ButtonFontSize"] = double.Parse(lb_main_skins.Resources["ButtonFontSize"].ToString()) * _widthMultiplier;

            localThickness = (Thickness)lb_main_skins.Resources["ButtonMargin"];
            localThickness.Top *= _heightMultiplier;
            localThickness.Bottom *= _heightMultiplier;
            localThickness.Left *= _widthMultiplier;
            localThickness.Right *= _widthMultiplier;
            lb_main_skins.Resources["ButtonMargin"] = localThickness;

            localThickness = (Thickness)lb_main_skins.Resources["ImageMargin"];
            localThickness.Top *= _heightMultiplier;
            localThickness.Bottom *= _heightMultiplier;
            localThickness.Left *= _widthMultiplier;
            localThickness.Right *= _widthMultiplier;
            lb_main_skins.Resources["ImageMargin"] = localThickness;

            WindowHelper.Resize(grid_main_skins_edit, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_main_skinsedit_preview, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_main_skinsedit_title, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(img_main_skinsedit_preview, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(bo_main_skinsedit_divider, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(scroll_main_skinsedit, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(grid_main_skinsedit_name, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_main_skinsedit_name, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_main_skinsedit_name_placeholder, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(tb_main_skinsedit_name, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(grid_main_skinsedit_model, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_main_skinsedit_model, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(cb_main_skinsedit_model, _heightMultiplier, _widthMultiplier);
            cb_main_skinsedit_model.Resources["FontSize"] = double.Parse(cb_main_skinsedit_model.Resources["FontSize"].ToString()) * _widthMultiplier;

            WindowHelper.Resize(grid_main_skinsedit_file, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_main_skinsedit_file, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_main_skinsedit_file, _heightMultiplier, _widthMultiplier);


            WindowHelper.Resize(grid_main_skinsedit_capes, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_main_skinsedit_capes, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(itemcont_main_skinsedit_capes, _heightMultiplier, _widthMultiplier);
            itemcont_main_skinsedit_capes.Resources["GridWidth"] = double.Parse(itemcont_main_skinsedit_capes.Resources["GridWidth"].ToString()) * _widthMultiplier;
            itemcont_main_skinsedit_capes.Resources["GridHeight"] = double.Parse(itemcont_main_skinsedit_capes.Resources["GridHeight"].ToString()) * _heightMultiplier;
            itemcont_main_skinsedit_capes.Resources["LabelFontSize"] = double.Parse(itemcont_main_skinsedit_capes.Resources["LabelFontSize"].ToString()) * _widthMultiplier;
            itemcont_main_skinsedit_capes.Resources["ButtonHeight"] = double.Parse(itemcont_main_skinsedit_capes.Resources["ButtonHeight"].ToString()) * _heightMultiplier;

            localThickness = (Thickness)itemcont_main_skinsedit_capes.Resources["GridMargin"];
            localThickness.Top *= _heightMultiplier;
            localThickness.Bottom *= _heightMultiplier;
            localThickness.Left *= _widthMultiplier;
            localThickness.Right *= _widthMultiplier;
            itemcont_main_skinsedit_capes.Resources["GridMargin"] = localThickness;

            localThickness = (Thickness)itemcont_main_skinsedit_capes.Resources["ImageMargin"];
            localThickness.Top *= _heightMultiplier;
            localThickness.Bottom *= _heightMultiplier;
            localThickness.Left *= _widthMultiplier;
            localThickness.Right *= _widthMultiplier;
            itemcont_main_skinsedit_capes.Resources["ImageMargin"] = localThickness;

            WindowHelper.Resize(bo_main_skinsedit_cancel, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(bo_main_skinsedit_save, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_main_skinsedit_save, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn__main_skinsedit_cancel, _heightMultiplier, _widthMultiplier);
            #endregion

            #region Home
            WindowHelper.Resize(bo_title_row, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(img_window_icon, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(l_WindowName, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(bt_window_close, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(bt_window_minimize, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(bo_leftmenu, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(gr_account, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(img_account, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(la_account_name, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(la_account_type, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(cb_launch_languages, _heightMultiplier, _widthMultiplier);
            cb_launch_languages.Resources["FontSize"] = double.Parse(cb_launch_languages.Resources["FontSize"].ToString()) * _widthMultiplier;

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

            listbox_launchinstances.Resources["ListBorderHeight"] = (double)listbox_launchinstances.Resources["ListBorderHeight"] * _heightMultiplier;
            listbox_launchinstances.Resources["ListBorderWidth"] = (double)listbox_launchinstances.Resources["ListBorderWidth"] * _widthMultiplier;

            localThickness = (Thickness)listbox_launchinstances.Resources["ListBorderMargin"];
            localThickness.Top *= _heightMultiplier;
            localThickness.Bottom *= _heightMultiplier;
            localThickness.Left *= _widthMultiplier;
            localThickness.Right *= _widthMultiplier;
            listbox_launchinstances.Resources["ListBorderMargin"] = localThickness;

            listbox_launchinstances.Resources["ListLabelHeight"] = (double)listbox_launchinstances.Resources["ListLabelHeight"] * _heightMultiplier;
            listbox_launchinstances.Resources["ListLabelWidth"] = (double)listbox_launchinstances.Resources["ListLabelWidth"] * _widthMultiplier;

            localThickness = (Thickness)listbox_launchinstances.Resources["ListLabelMargin"];
            localThickness.Top *= _heightMultiplier;
            localThickness.Bottom *= _heightMultiplier;
            localThickness.Left *= _widthMultiplier;
            localThickness.Right *= _widthMultiplier;
            listbox_launchinstances.Resources["ListLabelMargin"] = localThickness;

            listbox_launchinstances.Resources["ListLabelFontSize"] = (double)listbox_launchinstances.Resources["ListLabelFontSize"] * _widthMultiplier;

            listbox_launchinstances.Resources["ListButtonFontSize"] = (double)listbox_launchinstances.Resources["ListButtonFontSize"] * _widthMultiplier;
            listbox_launchinstances.Resources["ListButtonWidth"] = (double)listbox_launchinstances.Resources["ListButtonWidth"] * _widthMultiplier;
            listbox_launchinstances.Resources["ListButtonHeight"] = (double)listbox_launchinstances.Resources["ListButtonHeight"] * _heightMultiplier;

            localThickness = (Thickness)listbox_launchinstances.Resources["ListButtonMargin"];
            localThickness.Top *= _heightMultiplier;
            localThickness.Bottom *= _heightMultiplier;
            localThickness.Left *= _widthMultiplier;
            localThickness.Right *= _widthMultiplier;
            listbox_launchinstances.Resources["ListButtonMargin"] = localThickness;

            WindowHelper.Resize(bo_instances, _heightMultiplier, _widthMultiplier);
            #endregion
            #endregion
            #region Instances
            WindowHelper.ResizeFont(lab_instances, _heightMultiplier, _widthMultiplier);
            WindowHelper.Resize(lab_instances_widthfix, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(bo_instances_save, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_save, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_instances_save, _heightMultiplier, _widthMultiplier);

            WindowHelper.Resize(bo_instances_import, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(lab_instances_import, _heightMultiplier, _widthMultiplier);
            WindowHelper.ResizeFont(btn_instances_import, _heightMultiplier, _widthMultiplier);

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
            RefreshSkinsMenu();
            // Fill Languages ComboBox

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

            OpenInstanceEdit(null, string.Empty);
            LauncherSettings? settings = IOHelper.GetLauncherSettings();
            if (settings == null)
                throw new Exception("Restart the launcher.");
            // Load Languages
            cb_launch_languages.DataContext = TranslationManager.LanguagePacks;
            cb_launch_languages.SelectedIndex = TranslationManager.LanguagePacks.FindIndex(x => x.TwoLetterCode == settings.Language);
            //RefreshTranslations();
        }

        /// <summary>
        /// Handles the event when the window is loaded.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
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
                        la_account_type.Content = TranslationManager.Translate("ui_offline_account");
                        break;
                    }
                case EAccountType.MICROSOFT:
                    {
                        la_account_type.Content = TranslationManager.Translate("ui_microsoft_account");
                        break;
                    }
            }

            string avatarPath = System.IO.Path.Combine(IOHelper.CacheDir, $"{account.DisplayName}.png");
            if (!File.Exists(avatarPath))
            {
                using (var client = new HttpClient())
                {
                    img_account.Source = WindowHelper.GetImageSource(await client.GetByteArrayAsync($"https://mineskin.eu/head/{account.DisplayName}/256.png"));
                }
            }
            else
            {
                img_account.Source = await WindowHelper.GetImageSource(avatarPath);
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

            List<Profile> profiles = settings.Profiles.Values.ToList();

            if (profiles.Count <= 2)
                listbox_launchinstances.Resources["Alternation"] = profiles.Count;
            else
                listbox_launchinstances.Resources["Alternation"] = 2;
  
            if (settings.Profiles.TryGetValue(settings.SelectedProfile, out Profile? selectedProfile))
                SelectedProfile = new KeyValuePair<string, Profile>(settings.SelectedProfile, selectedProfile);
            else
                SelectedProfile = settings.Profiles.ElementAt(0);

            DataContext = settings.Profiles;
            listbox_launchinstances.SelectedIndex = profiles.IndexOf(SelectedProfile.Value);
            listbox_launchinstances.ScrollIntoView(listbox_launchinstances.SelectedItem);
            lab_selected_profile.Content = SelectedProfile.Value.Name.ToLower();


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
            if (manifest == null)
                return;
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
            foreach (var v in jObj["metadata"]?["versioning"]?["versions"]?["version"]?.ToList())
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

            List<VersionBase> localVanillaList = new List<VersionBase>();
            List<VersionBase> localModList = new List<VersionBase>();
            switch (versionType)
            {
                case "vanilla":
                    {
                        cb_instances_mc_version.DataContext = VersionDic[versionType].FindAll(x => (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas)).Select(x => x.Id);
                        cb_instances_mc_version.SelectedIndex = 0;
                        break;
                    }
                case "fabric":
                    {
                        localVanillaList = VersionDic["fabricVanilla"].FindAll(x => (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas));
                        cb_instances_mcmod_version.DataContext = localVanillaList.Select(x => x.Id);
                        cb_instances_mcmod_version.SelectedIndex = 0;

                        localModList = VersionDic[versionType];
                        cb_instances_mod_version.DataContext = localModList.Select(x => x.Id);
                        cb_instances_mod_version.SelectedIndex = 0;
                        break;
                    }
                case "quilt":
                    {
                        localVanillaList = VersionDic["quiltVanilla"].FindAll(x => (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas));
                        cb_instances_mcmod_version.DataContext = localVanillaList.Select(x => x.Id);
                        cb_instances_mcmod_version.SelectedIndex = 0;

                        localModList = VersionDic[versionType];
                        cb_instances_mod_version.DataContext = localModList.Select(x => x.Id);
                        cb_instances_mod_version.SelectedIndex = 0;
                        break;
                    }
                case "forge":
                    {
                        localVanillaList = VersionDic["forgeVanilla"].FindAll(x => !ForgeInstallerBase.UnsupportedVersions.Contains(x.VanillaId) && (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas));
                        cb_instances_mcmod_version.DataContext = localVanillaList.Select(x => x.Id);
                        cb_instances_mcmod_version.SelectedIndex = 0;

                        localModList = VersionDic[versionType].FindAll(x => !ForgeInstallerBase.UnsupportedVersions.Contains(x.VanillaId) && (x.VanillaId == localVanillaList[0].Id && (x.VersionType == EVersionType.RELEASE && showReleases) || (x.VersionType == EVersionType.SNAPSHOT && showSnapshots) || (x.VersionType == EVersionType.BETA && showOldBetas)));
                        cb_instances_mod_version.DataContext = localModList.Select(x => x.Id);
                        cb_instances_mod_version.SelectedIndex = localModList.FindIndex(x => x.VanillaId == localVanillaList[0].Id);
                        break;
                    }
            }
        }

        /// <summary>
        /// Refreshes the translations used in the application.
        /// </summary>
        public void RefreshTranslations()
        {
            // Main
            lab_new_instance.Content = TranslationManager.Translate("ui_new_instance");
            listbox_launchinstances.Resources["BtnTextEdit"] = TranslationManager.Translate("ui_instance_edit");
            listbox_launchinstances.Resources["BtnTextOpenDir"] = TranslationManager.Translate("ui_instance_opendir");
            listbox_launchinstances.Resources["BtnTextDel"] = TranslationManager.Translate("ui_instance_delete");
            listbox_launchinstances.Resources["BtnTextExportInst"] = TranslationManager.Translate("ui_instance_export");
            lab_launc_play.Content = TranslationManager.Translate("ui_play");

            // Home

            // Mods

            // Skins

            // Patches

            listbox_launchinstances.UpdateLayout();

            // Instance
            lab_instances.Content = TranslationManager.Translate("ui_new_instance");
            lab_instances_name.Content = TranslationManager.Translate("ui_instance_name");
            lab_instances_name_placeholder.Content = TranslationManager.Translate("ui_instance_name_placeholder");
            lab_instances_gamedir.Content = TranslationManager.Translate("ui_game_dir");
            lab_instances_gamedir_placeholder.Content = TranslationManager.Translate("ui_optional");
            lab_instances_javadir.Content = TranslationManager.Translate("ui_java_dir");
            lab_instances_javadir_placeholder.Content = TranslationManager.Translate("ui_optional");
            lab_instances_jvm.Content = TranslationManager.Translate("ui_jvm_args");
            lab_instances_jvm_placeholder.Content = TranslationManager.Translate("ui_optional");
            lab_instances_launchopt.Content = TranslationManager.Translate("ui_launcher_visibility");
            lab_instances_memory.Content = TranslationManager.Translate("ui_memory");
            lab_instances_resolution.Content = TranslationManager.Translate("ui_resolution");
            lab_instances_version.Content = TranslationManager.Translate("ui_version");
            checkb_instances_version_releases.Content = TranslationManager.Translate("ui_versioncb_releases");
            checkb_instances_version_snapshots.Content = TranslationManager.Translate("ui_versioncb_snapshots");
            checkb_instances_version_betas.Content = TranslationManager.Translate("ui_versioncb_betas");
            lab_instances_cancel.Content = TranslationManager.Translate("ui_cancel");
            lab_instances_save.Content = TranslationManager.Translate("ui_save");
            lab_instances_import.Content = TranslationManager.Translate("ui_orimport");
            btn_instances_javadir.Content = TranslationManager.Translate("ui_browse");
            btn_instances_gamedir.Content = TranslationManager.Translate("ui_browse");

        }

        /// <summary>
        /// Opens the instance editor for the specified profile.
        /// </summary>
        /// <param name="profile">The profile to edit.</param>
        /// <param name="profileKey">The key of the profile.</param>
        private void OpenInstanceEdit(Profile? profile, string profileKey)
        {
            if (profile == null)
            {
                EditedProfile = null;
                EditedProfileKey = string.Empty;

                scroll_instances.ScrollToTop();
                img_instances_icon.Source = new BitmapImage(new Uri(ProfileIcon.Icons.ElementAt(0).Path, UriKind.Relative));
                SelectedIcon = ProfileIcon.Icons.ElementAt(0).Path;
                cb_instances_mc_version.IsEnabled = true;
                cb_instances_mcmod_version.IsEnabled = false;
                cb_instances_mod_version.IsEnabled = false;
                cb_instances_mc_version.Visibility = Visibility.Visible;
                cb_instances_mcmod_version.Visibility = Visibility.Hidden;
                cb_instances_mod_version.Visibility = Visibility.Hidden;

                //RefreshDropdownVersions("vanilla");
                cb_instances_version_type.SelectedIndex = 0;
                cb_instances_mc_version.SelectedIndex = 0;


                tb_instances_gamedir.Text =  "";
                tb_instances_javadir.Text = "";
                tb_instances_name.Text = "";
                tb_instances_jvm.Text = Profile.GetDefaultJVMArgs();
                tb_instances_resolution_x.Text = string.Empty;
                tb_instances_resolution_y.Text = string.Empty;
                cb_instances_launchopt.SelectedIndex = 0;
                cb_instances_memory.SelectedIndex = 0;
                lab_instances.Content = TranslationManager.Translate("ui_new_instance");
                bo_instances_import.IsEnabled = true;
            }
            else
            {
                EditedProfile = profile;
                EditedProfileKey = profileKey;
                switch (EditedProfile.Kind)
                {
                    case EProfileKind.VANILLA:
                        {
                            
                            cb_instances_mc_version.IsEnabled = true;
                            cb_instances_mcmod_version.IsEnabled = false;
                            cb_instances_mod_version.IsEnabled = false;
                            cb_instances_mc_version.Visibility = Visibility.Visible;
                            cb_instances_mcmod_version.Visibility = Visibility.Hidden;
                            cb_instances_mod_version.Visibility = Visibility.Hidden;

                            cb_instances_version_type.SelectedValue = "Vanilla";
                            //RefreshDropdownVersions("vanilla");
                            cb_instances_mc_version.SelectedValue = EditedProfile.VersionId;

                            break;
                        }
                    case EProfileKind.FORGE:
                        {
                            
                            cb_instances_mc_version.IsEnabled = false;
                            cb_instances_mcmod_version.IsEnabled = true;
                            cb_instances_mod_version.IsEnabled = true;
                            cb_instances_mc_version.Visibility = Visibility.Hidden;
                            cb_instances_mcmod_version.Visibility = Visibility.Visible;
                            cb_instances_mod_version.Visibility = Visibility.Visible;

                            cb_instances_version_type.SelectedValue = "Forge";
                            //RefreshDropdownVersions("forge");
                            cb_instances_mcmod_version.SelectedValue = EditedProfile.VersionVanillaId;
                            cb_instances_mod_version.SelectedValue = EditedProfile.VersionId;
                            break;
                        }
                    case EProfileKind.FABRIC:
                        {
                            
                            cb_instances_mc_version.IsEnabled = false;
                            cb_instances_mcmod_version.IsEnabled = true;
                            cb_instances_mod_version.IsEnabled = true;
                            cb_instances_mc_version.Visibility = Visibility.Hidden;
                            cb_instances_mcmod_version.Visibility = Visibility.Visible;
                            cb_instances_mod_version.Visibility = Visibility.Visible;

                            cb_instances_version_type.SelectedValue = "Fabric";
                            //RefreshDropdownVersions("fabric");
                            cb_instances_mcmod_version.SelectedValue = EditedProfile.VersionVanillaId;
                            cb_instances_mod_version.SelectedValue = EditedProfile.VersionId;
                            break;
                        }
                    case EProfileKind.QUILT:
                        {
                            
                            cb_instances_mc_version.IsEnabled = false;
                            cb_instances_mcmod_version.IsEnabled = true;
                            cb_instances_mod_version.IsEnabled = true;
                            cb_instances_mc_version.Visibility = Visibility.Hidden;
                            cb_instances_mcmod_version.Visibility = Visibility.Visible;
                            cb_instances_mod_version.Visibility = Visibility.Visible;

                            cb_instances_version_type.SelectedValue = "Quilt";
                            //RefreshDropdownVersions("quilt");

                            cb_instances_mcmod_version.SelectedValue = EditedProfile.VersionVanillaId;
                            cb_instances_mod_version.SelectedValue = EditedProfile.VersionId;
                            break;
                        }
                }

                scroll_instances.ScrollToTop();
                SelectedIcon = profile.Icon.StartsWith("/assets") ? "pack://application:,,," + profile.Icon : profile.Icon;
                img_instances_icon.Source = new BitmapImage(new Uri(SelectedIcon));
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
                lab_instances.Content = TranslationManager.Translate("ui_edit_instance");
                bo_instances_import.IsEnabled = false;
            }
        }

        /// <summary>
        /// Updates the visibility of the launch status bar.
        /// </summary>
        /// <param name="isVisible">A boolean value indicating whether the launch status bar should be visible.</param>
        private void UpdateLaunchStatusBar(bool isVisible)
        {
            bo_launch_progress.IsEnabled = isVisible;
            bo_launch_progress.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Updates the launch status bar with the specified progress value and content.
        /// </summary>
        /// <param name="value">The progress value to display.</param>
        /// <param name="content">The content to display in the launch status bar.</param>
        private void UpdateLaunchStatusBar(double value, string content)
        {
            lab_launch_progress.Content = content;
            pb_launch_progress.Value = value;
        }

        /// <summary>
        /// Refreshes the skins menu in the application.
        /// </summary>
        private void RefreshSkinsMenu()
        {
            string raw = File.ReadAllText(IOHelper.SkinLibraryJsonFile);
            SkinLibData? skinLibData = JsonConvert.DeserializeObject<SkinLibData>(raw);
            if (skinLibData == null)
                return;

            lb_main_skins.DataContext = SkinLib.IncludeDefs(skinLibData.Skins);

            SkinLib? selectedSkin = skinLibData.Skins.Find(x => x.Id == skinLibData.SelectedSkin);
            if (selectedSkin == null)
            {
                btn_main_skins_addtolibrary.IsEnabled = true;
                img_main_skins_current.Source = WindowHelper.GetImageSourceFromUri("/assets/images/steve_full.png");
            }
            else
            {
                btn_main_skins_addtolibrary.IsEnabled = false;
                img_main_skins_current.Source = WindowHelper.GetImageSourceFromUri(selectedSkin.ModelImage);
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
            OpenInstanceEdit(null, string.Empty);
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

        /// <summary>
        /// Handles the event when the selection changes in the languages dropdown.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments containing information about the selection change.</param>
        private async void Languages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LauncherSettings? settings = IOHelper.GetLauncherSettings();
            if (settings == null)
                throw new Exception("Restart the launcher.");

            if (e.AddedItems.Count == 0)
                return;

            Language? item = e.AddedItems[0] as Language;
            if (item == null) 
                return;
            settings.Language = item.TwoLetterCode;
            await JsonHelper.WriteJsonFileAsync(IOHelper.LauncherJsonFile, settings);
            await TranslationManager.UpdateTranslations();
            RefreshTranslations();
        }

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
                NotificationHelper.SendErrorTranslated("launch_fail_account_details", "messagebox_error");
                return;
            }

            Account account = accountData.Accounts[accountData.SelectedAccountId];
            if (account == null)
            {
                NotificationHelper.SendErrorTranslated("launch_fail_current_account", "messagebox_error");
                return;
            }

            LauncherSettings? settings = IOHelper.GetLauncherSettings();
            if (settings == null)
            {
                NotificationHelper.SendErrorTranslated("launch_fail_settings", "messagebox_error");
                return;
            }

            Profile? selectedProfile = settings.Profiles[settings.SelectedProfile];
            if (selectedProfile == null)
            {
                NotificationHelper.SendErrorTranslated("launch_fail_invalid_profile", "messagebox_error");
                return;
            }
            #endregion

            UpdateLaunchStatusBar(true);
            UpdateLaunchStatusBar(0, $"Reading the vanillaManifest file...");
            MinecraftInstaller installer;
            bool debug = false;
#if DEBUG
            debug = true;
#endif
            switch (selectedProfile.Kind)
            {
                default:
                case EProfileKind.VANILLA:
                    {
                        installer = new MinecraftInstaller(selectedProfile, lab_launch_progress, pb_launch_progress, debug);
                        break;
                    }
                case EProfileKind.FORGE:
                    {
                        ForgeInstallerBase? localInstaller = ForgeInstallerBase.Create(selectedProfile, lab_launch_progress, pb_launch_progress, debug);

                        if (localInstaller == null)
                        {
                            // Enable play button
                            bo_launch_play.IsEnabled = true;
                            btn_launch_play.IsEnabled = true;
                            // Disable progressbar
                            UpdateLaunchStatusBar(false);

                            NotificationHelper.SendWarningTranslated("minecraft_version_unsupported", "messagebox_warning", new object[] { selectedProfile.VersionVanillaId });
                            return;
                        }

                        installer = localInstaller;
                        break;
                    }
                case EProfileKind.FABRIC:
                    {
                        installer = new FabricInstaller(selectedProfile, lab_launch_progress, pb_launch_progress, debug);
                        break;
                    }
                case EProfileKind.QUILT:
                    {
                        
                        installer = new QuiltInstaller(selectedProfile, lab_launch_progress, pb_launch_progress, debug);
                        break;
                    }
            }

            Process process = await installer.Install();

            // Enable play button
            bo_launch_play.IsEnabled = true;
            btn_launch_play.IsEnabled = true;
            // Disable progressbar
            UpdateLaunchStatusBar(false);

            switch (selectedProfile.LauncherVisibility)
            {
                case ELaucnherVisibility.HIDE_AND_REOPEN_ON_GAME_CLOSE:
                    {
                        this.Hide();
                        if (process != null)
                            await process.WaitForExitAsync();
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

            if (debug)
            {
                string o = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(o))
                    NotificationHelper.SendErrorMsg(o, "Error - Minecraft");
            }

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

            ListBox? listBox = e.Source as ListBox;
            if (listBox == null)
                return;
            try
            {
                KeyValuePair<string, Profile>? addedItem = (KeyValuePair<string, Profile>?)e.AddedItems[0];
                listBox.Resources["SelectedIndex"] = listBox.Items.IndexOf(addedItem);

                LauncherSettings? settings = IOHelper.GetLauncherSettings();
                if (settings != null)
                {
                    var p = settings.Profiles.ElementAt(listbox_launchinstances.SelectedIndex);
                    lab_selected_profile.Content = p.Value.Name.ToLower();
                    settings.SelectedProfile = p.Key;
                    await JsonHelper.WriteJsonFileAsync(Path.Combine(IOHelper.MainDirectory, "launcher.json"), settings);
                }
            }
            catch { }
        }

        /// <summary>
        /// Handles the event when the selection changes in the instance action box.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments containing information about the selection change.</param>
        private async void InstanceActionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            var item = e.AddedItems[0] as ComboBoxItem;
            if (item == null)
                return;


            switch (item.Name)
            {
                case "edit":
                    {
                        KeyValuePair<string, Profile> profile = (KeyValuePair<string, Profile>)item.Tag;
                        OpenInstanceEdit(profile.Value, profile.Key);
                        bo_instances.Visibility = Visibility.Visible;
                        bo_instances.IsEnabled = true;
                        break;
                    }
                case "exportinst":
                    {
                        KeyValuePair<string, Profile> profile = (KeyValuePair<string, Profile>)item.Tag;
                        System.Windows.Forms.SaveFileDialog fileDialog = new System.Windows.Forms.SaveFileDialog();
                        fileDialog.Title = "Select save location";
                        fileDialog.AddExtension = true;
                        fileDialog.CheckFileExists = false;
                        fileDialog.CheckPathExists = true;
                        fileDialog.DefaultExt = ".zip";
                        fileDialog.Filter = "(*.zip)|*.zip";
                        var dialogResult = fileDialog.ShowDialog();
                        if (dialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            await InstanceManager.ExportKonkordInstance(profile.Value, fileDialog.FileName);
                        }
                        break;
                    }
                case "opendir":
                    {
                        KeyValuePair<string, Profile> profile = (KeyValuePair<string, Profile>)item.Tag;
                        string dir = profile.Value.GetGameDirectory();
                        if (!Directory.Exists(dir))
                        {
                            // TODO, add methods for other applications
                            // at the moment the focus is on windows, so idk when I will add it.
                            Process.Start("explorer.exe", IOHelper.InstancesDir);
                            return;
                        }

                        Process.Start("explorer.exe", dir);
                        break;
                    }
                case "delete":
                    {
                        KeyValuePair<string, Profile> profile = (KeyValuePair<string, Profile>)item.Tag;

                        MessageBoxResult result = MessageBox.Show($"Are you sure about deleting the '{profile.Value.Name}' profile?", "Profile deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            LauncherSettings? settings = IOHelper.GetLauncherSettings();
                            if (settings == null)
                                return;

                            settings.Profiles.Remove(profile.Key);
                            await JsonHelper.WriteJsonFileAsync(IOHelper.LauncherJsonFile, settings);
                            string dir = profile.Value.GetGameDirectory();
                            if (Directory.Exists(dir))
                                IOHelper.DeleteDirectory(dir);
                            RefreshInstances();
                        }
                        break;
                    }
            }

            ComboBox? comboBox = sender as ComboBox;
            if (comboBox != null)
                comboBox.SelectedIndex = -1;
        }

        #region Category Menu
        private void TopmenuHome_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in dp_category.Children)
            {
                if (child is not Button button)
                    continue;

                if (sender == button)
                    button.Style = (Style)FindResource("CategoryButtonSelected");
                else
                    button.Style = (Style)FindResource("CategoryButton");
            }

            foreach (var child in grid_main.Children)
            {
                if (child is not Grid grid)
                    continue;

                grid.Visibility = grid.Name == "grid_main_home" ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void TopmenuModPacks_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in dp_category.Children)
            {
                if (child is not Button button)
                    continue;

                if (sender == button)
                    button.Style = (Style)FindResource("CategoryButtonSelected");
                else
                    button.Style = (Style)FindResource("CategoryButton");
            }

            foreach (var child in grid_main.Children)
            {
                if (child is not Grid grid)
                    continue;

                grid.Visibility = grid.Name == "grid_main_modpacks" ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void TopmenuSkins_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in dp_category.Children)
            {
                if (child is not Button button)
                    continue;

                if (sender == button)
                    button.Style = (Style)FindResource("CategoryButtonSelected");
                else
                    button.Style = (Style)FindResource("CategoryButton");
            }

            foreach (var child in grid_main.Children)
            {
                if (child is not Grid grid)
                    continue;

                grid.Visibility = grid.Name == "grid_main_skins" ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void TopmenuPatchs_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in dp_category.Children)
            {
                if (child is not Button button)
                    continue;

                if (sender == button)
                    button.Style = (Style)FindResource("CategoryButtonSelected");
                else
                    button.Style = (Style)FindResource("CategoryButton");
            }

            foreach (var child in grid_main.Children)
            {
                if (child is not Grid grid)
                    continue;

                grid.Visibility = grid.Name == "grid_main_patchs" ? Visibility.Visible : Visibility.Hidden;
            }
        }
        #endregion
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
                NotificationHelper.SendErrorTranslated("instance_name_empty", "messagebox_error");
                return;
            }

            if ((cb_instances_mc_version.IsEnabled && cb_instances_mc_version.SelectedIndex < 0) || (cb_instances_mcmod_version.IsEnabled && (cb_instances_mcmod_version.SelectedIndex < 0 || cb_instances_mod_version.SelectedIndex < 0)))
            {
                NotificationHelper.SendErrorTranslated("version_empty", "messagebox_error");
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
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
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

        private async void InstancesImport_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = "Select the instance.zip";
            dialog.DefaultExt = "zip";
            dialog.Filter = "(*.zip)|*.zip";
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (!File.Exists(dialog.FileName))
                    return;

                // Hide instance editor
                bo_instances.IsEnabled = false;
                bo_instances.Visibility = Visibility.Hidden;

                // Disable play button
                bo_launch_play.IsEnabled = false;
                btn_launch_play.IsEnabled = false;
                // Enable progressbar
                UpdateLaunchStatusBar(true);
                UpdateLaunchStatusBar(0, $"Importing instance...");

                await InstanceManager.HandleInstanceZipImport(dialog.FileName, pb_launch_progress, lab_launch_progress);

                RefreshInstances();

                // Enable play button
                bo_launch_play.IsEnabled = true;
                btn_launch_play.IsEnabled = true;
                // Disable progressbar
                UpdateLaunchStatusBar(false);
            }
        }
        #endregion

        private void MainSkinsItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = (ListBox)sender;

            if (e.RemovedItems != null)
            {
                foreach (object selectedItem in e.RemovedItems)
                {
                    ListBoxItem? selectedListBoxItem = listBox.ItemContainerGenerator.ContainerFromItem(selectedItem) as ListBoxItem;

                    if (selectedListBoxItem == null)
                        return;
                    Grid grid = WindowHelper.FindVisualChild<Grid>(selectedListBoxItem);
                    SelectedSkinGrid = grid;

                    foreach (var child in grid.Children)
                    {
                        if (child is Image image)
                        {
                            image.Opacity = 0.5;
                        }
                        else if (child is Border button)
                        {
                            button.Visibility = Visibility.Hidden;
                        }
                        else if (child is ComboBox comboBox)
                        {
                            comboBox.Visibility = Visibility.Hidden;
                        }
                    }
                }
            }

            if (e.AddedItems != null)
            {
                foreach (object selectedItem in e.AddedItems)
                {
                    ListBoxItem? selectedListBoxItem = listBox.ItemContainerGenerator.ContainerFromItem(selectedItem) as ListBoxItem;

                    if (selectedListBoxItem == null)
                        return;
                    Grid grid = WindowHelper.FindVisualChild<Grid>(selectedListBoxItem);
                    SelectedSkinGrid = grid;

                    foreach (var child in grid.Children)
                    {
                        if (child is Image image && grid != SelectedSkinGrid)
                        {
                            image.Opacity = 0.5;
                        }
                        else if (child is Border button && grid != SelectedSkinGrid)
                        {
                            button.Visibility = Visibility.Hidden;
                        }
                        else if (child is ComboBox comboBox && grid != SelectedSkinGrid)
                        {
                            comboBox.Visibility = Visibility.Hidden;
                        }
                    }
                }
            }

            
        }

        private void SkinListGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid grid = (Grid)sender;

            foreach (var child in grid.Children)
            {
                if (child is Image image)
                {
                    image.Opacity = 1d;
                }
                else if (child is Border button)
                {
                    button.Visibility = Visibility.Visible;
                }
                else if (child is ComboBox comboBox)
                {
                    comboBox.Visibility = Visibility.Visible;
                }
            }
        }

        private void SkinListGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid grid = (Grid)sender;

            foreach (var child in grid.Children)
            {
                if (child is Image image && grid != SelectedSkinGrid)
                {
                    image.Opacity = 0.5;
                }
                else if (child is Border button && grid != SelectedSkinGrid)
                {
                    button.Visibility = Visibility.Hidden;
                }
                else if (child is ComboBox comboBox && grid != SelectedSkinGrid)
                {
                    comboBox.Visibility = Visibility.Hidden;
                }
            }
        }

        private void CapeRadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_main_skins_newskin_cancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_main_skins_newskin_Save_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void SkinsAddToLibrary_Click(object sender, RoutedEventArgs e)
        {
            AccountData? accountsData = await JsonHelper.ReadJsonFileAsync<AccountData?>(IOHelper.AccountsJsonFile);
            if (accountsData == null)
                return;

            if (!accountsData.Accounts.TryGetValue(accountsData.SelectedAccountId, out Account? account))
                return;

            if (account.Type != EAccountType.MICROSOFT)
                return;

            MojangProfile? mojangProfile = await AuthenticationManager.GetMojangProfileAsync(account.AccessToken);
            if (mojangProfile == null)
                return;

            Skin? skin = mojangProfile.Skins.Find(x => x.State == "ACTIVE");
            if (skin == null)
                return;

            string raw = File.ReadAllText(IOHelper.SkinLibraryJsonFile);
            SkinLibData? skinLibData = JsonConvert.DeserializeObject<SkinLibData>(raw);
            if (skinLibData == null)
                return;
        }

        private void SkinsNewSkin_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

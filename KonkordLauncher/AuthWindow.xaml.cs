using Tavstal.KonkordLibrary.Enums;
using Tavstal.KonkordLibrary.Helpers;
using Tavstal.KonkordLibrary.Managers;
using Tavstal.KonkordLibrary.Models.Launcher;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Tavstal.KonkordLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
            #region Resize Content
            // Gets the primary screen parameters.
            int nWidth = (int)SystemParameters.PrimaryScreenWidth;
            int nHeight = (int)SystemParameters.PrimaryScreenHeight;

            double oldHeight = Height;
            double oldWidth = Width;
            Height = nHeight * 0.6; // 0.55 means 55% of the screen's height
            Width = nWidth * 0.6; // 0.55 means 55% of the screen's width

            // Get the multiplier how much should the window's content to be resized.
            double heightMultiplier = Height / oldHeight; 
            double widthMultiplier = Width / oldWidth;

            WindowHelper.Resize(bo_title_row, heightMultiplier, widthMultiplier);
            WindowHelper.Resize(img_window_icon, heightMultiplier, widthMultiplier);
            WindowHelper.ResizeFont(l_WindowName, heightMultiplier, widthMultiplier);
            WindowHelper.ResizeFont(bt_window_close, heightMultiplier, widthMultiplier);
            WindowHelper.ResizeFont(bt_window_minimize, heightMultiplier, widthMultiplier);

            WindowHelper.Resize(auth_offline_border, heightMultiplier, widthMultiplier);
            WindowHelper.Resize(img_offline_logo, heightMultiplier, widthMultiplier);
            WindowHelper.Resize(bo_auth_offline_buy, heightMultiplier, widthMultiplier);
            WindowHelper.Resize(bo_auth_offline_login, heightMultiplier, widthMultiplier);
            WindowHelper.Resize(bo_auth_offline_switch, heightMultiplier, widthMultiplier);
            WindowHelper.Resize(bo_auth_offline_username, heightMultiplier, widthMultiplier);

            WindowHelper.ResizeFont(btn_auth_offline_buy, heightMultiplier, widthMultiplier);
            WindowHelper.ResizeFont(btn_auth_offline_login, heightMultiplier, widthMultiplier);
            WindowHelper.ResizeFont(btn_auth_offline_switch, heightMultiplier, widthMultiplier);
            WindowHelper.ResizeFont(tb_auth_offline_username, heightMultiplier, widthMultiplier);

            WindowHelper.Resize(auth_online_border, heightMultiplier, widthMultiplier);
            WindowHelper.Resize(img_online_logo, heightMultiplier, widthMultiplier);
            WindowHelper.Resize(bo_auth_online_buy, heightMultiplier, widthMultiplier);
            WindowHelper.Resize(bo_auth_online_login, heightMultiplier, widthMultiplier);
            WindowHelper.Resize(bo_auth_online_switch, heightMultiplier, widthMultiplier);

            WindowHelper.ResizeFont(btn_auth_online_buy, heightMultiplier, widthMultiplier);
            WindowHelper.ResizeFont(btn_auth_online_login, heightMultiplier, widthMultiplier);
            WindowHelper.ResizeFont(btn_auth_online_switch, heightMultiplier, widthMultiplier);
            #endregion

            RefreshTranslations();
        }

        public void RefreshTranslations()
        {
            lab_auth_offline_username.Content = TranslationManager.Translate("ui_username");
            btn_auth_offline_buy.Content = TranslationManager.Translate("ui_auth_buy_minecraft");
            btn_auth_offline_login.Content = TranslationManager.Translate("ui_auth_play_offline");
            btn_auth_offline_switch.Content = TranslationManager.Translate("ui_auth_switch_to_online");
            btn_auth_online_buy.Content = TranslationManager.Translate("ui_auth_buy_minecraft");
            btn_auth_online_login.Content = TranslationManager.Translate("ui_auth_login_microsoft");
            btn_auth_online_switch.Content = TranslationManager.Translate("ui_auth_switch_to_offline");
        }

        #region Events
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

        /// <summary>
        /// Event handler for performing a buy action when the offline buy button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OfflineBuy_Click(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "https://www.minecraft.net/",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        /// <summary>
        /// Event handler for switching between online and offline modes when the online switch button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnlineSwitch_Click(object sender, RoutedEventArgs e)
        {
            auth_offline_border.IsEnabled = true;
            auth_offline_border.Visibility = Visibility.Visible;

            auth_online_border.IsEnabled = false;
            auth_online_border.Visibility = Visibility.Hidden;

            img_offline_logo.Visibility = Visibility.Visible;
            img_offline_logo.IsEnabled = true;

            img_online_logo.Visibility = Visibility.Hidden;
            img_online_logo.IsEnabled = false;
        }

        /// <summary>
        /// Event handler for switching between online and offline modes when the offline switch button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OfflineSwitch_Click(object sender, RoutedEventArgs e)
        {
            auth_offline_border.IsEnabled = false;
            auth_offline_border.Visibility = Visibility.Hidden;

            auth_online_border.IsEnabled = true;
            auth_online_border.Visibility = Visibility.Visible;

            img_offline_logo.Visibility = Visibility.Hidden;
            img_offline_logo.IsEnabled = false;

            img_online_logo.Visibility = Visibility.Visible;
            img_online_logo.IsEnabled = true;
        }

        /// <summary>
        /// Event handler for initiating the offline login process when the offline login button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private async void OfflineLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = tb_auth_offline_username.Text;

            if (string.IsNullOrEmpty(username))
            {
                NotificationHelper.SendErrorTranslated("username_is_empty", "messagebox_error");
                return;
            }

            if (username.Length <= 2)
            {
                NotificationHelper.SendErrorTranslated("username_too_short", "messagebox_error");
                return;
            }

            if (username.Length > 16)
            {
                NotificationHelper.SendErrorTranslated("username_too_long", "messagebox_error");
                return;
            }

            string accountsPath = Path.Combine(IOHelper.MainDirectory, "accounts.json");
            AccountData? accountData = await JsonHelper.ReadJsonFileAsync<AccountData>(accountsPath);
            if (accountData == null)
            {
                NotificationHelper.SendErrorTranslated("file_not_found", "messagebox_error", new object[] { "accounts.json" });
                return;
            }

            string guid = GameHelper.GetOfflinePlayerUUID(username);
            if (!accountData.Accounts.ContainsKey(guid))
            {
                accountData.Accounts.Add(guid, new Account()
                {
                    AccessToken = string.Empty,
                    RefreshToken = string.Empty,
                    DisplayName = username,
                    Type = EAccountType.OFFLINE,
                    UserId = guid,
                    UUID = guid
                });
            }
            accountData.SelectedAccountId = guid;
            await JsonHelper.WriteJsonFileAsync(accountsPath, accountData);
            LaunchWindow window = new LaunchWindow();
            window.Show();
            this.Close();
        }

        /// <summary>
        /// Event handler for when the offline username input field receives focus.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OfflineUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            lab_auth_offline_username.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Event handler for when the offline username input field loses focus.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OfflineUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb_auth_offline_username.Text))
                lab_auth_offline_username.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the click event of the online login button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private async void OnlineLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btn_auth_online_login.IsEnabled = false;
                btn_auth_online_switch.IsEnabled = false;
                var psi = new ProcessStartInfo
                {
                    FileName = AuthenticationManager.MicrosoftAuthUrl,
                    UseShellExecute = true
                };
                Process.Start(psi);
                AuthenticationManager.StartListening();

                while (AuthenticationManager.IsListening)
                {
                    await Task.Delay(50);
                    if (AuthenticationManager.GetMicrosoftAuthStatus())
                    {
                        LaunchWindow window = new LaunchWindow();
                        window.Show();
                        this.Close();
                    }
                }

                btn_auth_online_login.IsEnabled = true;
                btn_auth_online_switch.IsEnabled = true;
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "Online Login Error");
            }
        }
        #endregion
    }
}
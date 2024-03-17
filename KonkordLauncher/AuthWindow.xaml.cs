using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using KonkordLauncher.API.Helpers;
using KonkordLauncher.API.Managers;
using KonkordLauncher.API.Models;

namespace KonkordLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
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

            Resize(auth_offline_border, heightMultiplier, widthMultiplier);
            Resize(img_offline_logo, heightMultiplier, widthMultiplier);
            Resize(bo_auth_offline_buy, heightMultiplier, widthMultiplier);
            Resize(bo_auth_offline_login, heightMultiplier, widthMultiplier);
            Resize(bo_auth_offline_switch, heightMultiplier, widthMultiplier);
            Resize(bo_auth_offline_username, heightMultiplier, widthMultiplier);

            ResizeFont(btn_auth_offline_buy, heightMultiplier, widthMultiplier);
            ResizeFont(btn_auth_offline_login, heightMultiplier, widthMultiplier);
            ResizeFont(btn_auth_offline_switch, heightMultiplier, widthMultiplier);
            ResizeFont(tb_auth_offline_username, heightMultiplier, widthMultiplier);

            Resize(auth_online_border, heightMultiplier, widthMultiplier);
            Resize(img_online_logo, heightMultiplier, widthMultiplier);
            Resize(bo_auth_online_buy, heightMultiplier, widthMultiplier);
            Resize(bo_auth_online_login, heightMultiplier, widthMultiplier);
            Resize(bo_auth_online_switch, heightMultiplier, widthMultiplier);

            ResizeFont(btn_auth_online_buy, heightMultiplier, widthMultiplier);
            ResizeFont(btn_auth_online_login, heightMultiplier, widthMultiplier);
            ResizeFont(btn_auth_online_switch, heightMultiplier, widthMultiplier);
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

        #endregion

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
                NotificationHelper.SendError("You must provide a valid username.", "Error");
                return;
            }

            if (username.Length > 16)
            {
                NotificationHelper.SendError("Your username must be equal or shorter than 16 characters.", "Error");
                return;
            }

            Guid guid = Guid.NewGuid();
            string accountsPath = Path.Combine(IOHelper.MainDirectory, "accounts.json");
            AccountData? accountData = await JsonHelper.ReadJsonFile<AccountData>(accountsPath);
            if (accountData == null)
            {
                NotificationHelper.SendError("Failed to get the accounts.json file.", "Error");
                return;
            }

            accountData.Accounts.Add(guid.ToString(), new Account()
            {
                AccessToken = string.Empty,
                RefreshToken = string.Empty,
                DisplayName = username,
                Type = API.Enums.EAccountType.OFFLINE,
                UserId = guid.ToString(),
                UUID = guid.ToString()
            });
            accountData.SelectedAccountId = guid.ToString();
            await JsonHelper.WriteJsonFile(accountsPath, accountData);
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
        #endregion

        private void OnlineLogin_Click(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = AuthenticationManager.MicrosoftAuthorizeUrl,
                UseShellExecute = true
            };
            Process.Start(psi);
            AuthenticationManager.StartListening();
        }
    }
}
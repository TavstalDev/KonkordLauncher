using FontAwesome.WPF;
using System.Diagnostics;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            int nWidth = (int)SystemParameters.PrimaryScreenWidth;
            int nHeight = (int)SystemParameters.PrimaryScreenHeight;

            double oldHeight = Height;
            double oldWidth = Width;
            Height = nHeight * 0.55;
            Width = nWidth * 0.55;

            double heightMultiplier = Height / oldHeight;
            double widthMultiplier = Width / oldWidth;

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

        private void Resize(FrameworkElement element, double heightMulti, double widthMulti)
        {
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine("Excuse me? -> " + element.Name);
            }
        }

        private void ResizeFont(Control element, double heightMulti, double widthMulti)
        {
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine("Excuse me? -> " + element.Name);
            }
        }

        private void WindowClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WindowMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void WindowRestore_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            bt_window_maximize.IsEnabled = true;
            bt_window_normal.IsEnabled = false;
            bt_window_normal.Visibility = Visibility.Hidden;
            bt_window_maximize.Visibility = Visibility.Visible;
        }

        private void WindowMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            bt_window_maximize.IsEnabled = false;
            bt_window_normal.IsEnabled = true;
            bt_window_normal.Visibility = Visibility.Visible;
            bt_window_maximize.Visibility = Visibility.Hidden;
        }

        private void bt_auth_offline_buy_Click(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "https://www.minecraft.net/",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void btn_auth_online_switch_Click(object sender, RoutedEventArgs e)
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
    }
}
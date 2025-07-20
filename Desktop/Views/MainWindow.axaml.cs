using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Desktop.Enums;
using Tavstal.KonkordLauncher.Desktop.ViewModels;

namespace Tavstal.KonkordLauncher.Desktop.Views;

public partial class MainWindow : Window
{
    private Button _selectedButton;
    
    public MainWindow()
    {
        InitializeComponent();
        
#if DEBUG
        this.AttachDevTools(); // Attaches Avalonia Dev Tools for debugging
#endif
        // Instantiate your ViewModel and assign it to the DataContext
        this.DataContext = new MainWindowViewModel();
        _selectedButton = PlaySideBtn;

    }
    
    public void OnPlaySideButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            if (viewModel.CurrentPageIndex == ESidebarType.Play)
                return;
            
            viewModel.CurrentPageIndex = ESidebarType.Play;
            _selectedButton.Classes.Remove("SidebarSelectedBtn");
            _selectedButton.Classes.Add("SidebarBtn");
            PlaySideBtn.Classes.Remove("SidebarBtn");
            PlaySideBtn.Classes.Add("SidebarSelectedBtn");
            _selectedButton = PlaySideBtn;
        }
    }
    
    public void OnInstancesSideButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            if (viewModel.CurrentPageIndex == ESidebarType.Instances)
                return;
            
            viewModel.CurrentPageIndex = ESidebarType.Instances;
            _selectedButton.Classes.Remove("SidebarSelectedBtn");
            _selectedButton.Classes.Add("SidebarBtn");
            InstancesSideBtn.Classes.Remove("SidebarBtn");
            InstancesSideBtn.Classes.Add("SidebarSelectedBtn");
            _selectedButton = InstancesSideBtn;
        }
    }
    
    public void OnNewsSideButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            if (viewModel.CurrentPageIndex == ESidebarType.News)
                return;
            
            viewModel.CurrentPageIndex = ESidebarType.News;
            _selectedButton.Classes.Remove("SidebarSelectedBtn");
            _selectedButton.Classes.Add("SidebarBtn");
            NewsSideBtn.Classes.Remove("SidebarBtn");
            NewsSideBtn.Classes.Add("SidebarSelectedBtn");
            _selectedButton = NewsSideBtn;
        }
    }
    
    public void OnAccountsSideButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            if (viewModel.CurrentPageIndex == ESidebarType.Accounts)
                return;
            
            viewModel.CurrentPageIndex = ESidebarType.Accounts;
            _selectedButton.Classes.Remove("SidebarSelectedBtn");
            _selectedButton.Classes.Add("SidebarBtn");
            AccountsSideBtn.Classes.Remove("SidebarBtn");
            AccountsSideBtn.Classes.Add("SidebarSelectedBtn");
            _selectedButton = AccountsSideBtn;
        }
    }
    
    public void OnSettingsSideButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            if (viewModel.CurrentPageIndex == ESidebarType.Settings)
                return;
            
            viewModel.CurrentPageIndex = ESidebarType.Settings;
            _selectedButton.Classes.Remove("SidebarSelectedBtn");
            _selectedButton.Classes.Add("SidebarBtn");
            SettingsSideBtn.Classes.Remove("SidebarBtn");
            SettingsSideBtn.Classes.Add("SidebarSelectedBtn");
            _selectedButton = SettingsSideBtn;
        }
    }
}
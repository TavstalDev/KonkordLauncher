using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Desktop.Enums;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.ViewModels;

namespace Tavstal.KonkordLauncher.Desktop.Views;

// ReSharper disable once PartialTypeWithSinglePart
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
    
    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        UpdateInstancesOnPlayPage();
    }

    #region Methods

    private void HandleSidebarChange(ESidebarType sidebarType)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;
        
        if (viewModel.CurrentPageIndex == sidebarType)
            return;

        viewModel.CurrentPageIndex = sidebarType;
        _selectedButton.Classes.Remove("PrimaryBtn");
        _selectedButton.Classes.Add("SecondaryBtn");

        switch (sidebarType)
        {
            case ESidebarType.Play:
            {
                
                _selectedButton = PlaySideBtn;
                break;
            }
            case ESidebarType.News:
            {
                _selectedButton = NewsSideBtn;
                break;
            }
            case ESidebarType.Accounts:
            {
                _selectedButton = AccountsSideBtn;
                break;
            }
            case ESidebarType.Settings:
            {
                _selectedButton = SettingsSideBtn;
                break;
            }
        }
        _selectedButton.Classes.Remove("SecondaryBtn");
        _selectedButton.Classes.Add("PrimaryBtn");
    }

    private void UpdateInstancesOnPlayPage()
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        // TODO: Here you would typically fetch or update the instances on the play page.
        // For demonstration, let's assume we are adding a new instance.
        viewModel.InstancesOnPlayPage.Add(new PlayCardModel { Title = "New Instance" });
        
        // After Updating:
        bool hasInstances = viewModel.InstancesOnPlayPage.Count > 0;
        NoPlayInstancesTextBlock.IsVisible = !hasInstances;
    }
    #endregion

    #region Event Handlers

    public void OnPlaySideButtonClick(object? sender, RoutedEventArgs e)
    {
        HandleSidebarChange(ESidebarType.Play);
    }
    
    public void OnNewsSideButtonClick(object? sender, RoutedEventArgs e)
    {
        HandleSidebarChange(ESidebarType.News);
    }
    
    public void OnAccountsSideButtonClick(object? sender, RoutedEventArgs e)
    {
        HandleSidebarChange(ESidebarType.Accounts);
    }
    
    public void OnSettingsSideButtonClick(object? sender, RoutedEventArgs e)
    {
        HandleSidebarChange(ESidebarType.Settings);
    }

    #endregion
}
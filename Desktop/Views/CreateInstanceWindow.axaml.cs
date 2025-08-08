using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents the window for creating a new instance in the application.
/// Initializes the UI components, attaches Avalonia Dev Tools in debug mode,
/// and sets the DataContext to a new <see cref="CreateInstanceViewModel"/>.
/// </summary>
public partial class CreateInstanceWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateInstanceWindow"/> class.
    /// Sets up the UI components, attaches dev tools in debug mode, and assigns the DataContext.
    /// </summary>
    public CreateInstanceWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        this.DataContext = new CreateInstanceViewModel();
    }

    #region Custom - Mod Loader Select
    /// <summary>
    /// Handles the click event for the "No Mod Loader" option.
    /// Sets the ModLoaderType of the ViewModel to VANILLA.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CustomNoModLoader_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel vm) return;

        vm.ModLoaderType = EMinecraftKind.VANILLA;
    }

    /// <summary>
    /// Handles the click event for the "NeoForge" mod loader option.
    /// Sets the ModLoaderType of the ViewModel to NEOFORGE.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CustomNeoForge_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel vm) return;

        vm.ModLoaderType = EMinecraftKind.NEOFORGE;
    }

    /// <summary>
    /// Handles the click event for the "Forge" mod loader option.
    /// Sets the ModLoaderType of the ViewModel to FORGE.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CustomForge_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel vm) return;

        vm.ModLoaderType = EMinecraftKind.FORGE;
    }

    /// <summary>
    /// Handles the click event for the "Fabric" mod loader option.
    /// Sets the ModLoaderType of the ViewModel to FABRIC.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CustomFabric_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel vm) return;

        vm.ModLoaderType = EMinecraftKind.FABRIC;
    }

    /// <summary>
    /// Handles the click event for the "Quilt" mod loader option.
    /// Sets the ModLoaderType of the ViewModel to QUILT.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CustomQuilt_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel vm) return;

        vm.ModLoaderType = EMinecraftKind.QUILT;
    }
    #endregion

    /// <summary>
    /// Handles the click event for the icon selector.
    /// Opens the IconSelectorWindow dialog and sets the selected icon in the ViewModel if one is chosen.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private async void IconSelector_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Replace async void
        if (DataContext is not CreateInstanceViewModel vm)
            return;

        IconSelectorWindow window = new();
        var result = await window.ShowDialog<IconDataModel>(this);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (result == null)
            return;
        vm.InstanceIcon = result.Image;
        vm.InstanceIconPath = result.Path;
    }

    /// <summary>
    /// Handles the click event for creating a new instance.
    /// Validates the instance name for duplicates, adds the new instance to the list,
    /// saves the updated list to a JSON file, and notifies the application of the change.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CreateInstance_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel vm)
            return;

        var settings = LauncherHelper.GetLauncherSettings();
        var instances = LauncherHelper.GetInstances();
        if (instances.Any(x => x.Name == vm.InstanceName))
        {
            AlertWindow alertWindow = new(TranslationManager.Translate("instance.duplicate.title"),
                TranslationManager.Translate("instance.duplicate.message"),
                EAlertType.Error);
            alertWindow.ShowDialog(this);
            return;
        }
        
        instances.Add(new Instance
        {
            Name = vm.InstanceName,
            Kind = vm.ModLoaderType,
            Group = "none",
            MinecraftVersion = vm.SelectedMinecraftVersion.Id,
            CustomVersion = vm.SelectedModLoader?.Version ?? string.Empty,
            IconPath = vm.InstanceIconPath ?? string.Empty,
            GameDirectory = System.IO.Path.Combine(settings.Launcher.InstancesDirectoryPath, vm.InstanceName),
            Config = new InstanceConfig()
            {
                Game = new InstanceGameConfig()
                {
                    StartMaximized = settings.Minecraft.StartMaximized,
                    WindowHeight = (uint)(0.45 * App.ScreenSize.Height),
                    WindowWidth = (uint)(0.40 * App.ScreenSize.Width),
                    ShowConsoleWhenGameCrashes = true,
                    ShowConsoleWhileGameRunning = false,
                    CloseConsoleOnGameExit = false,
                    EnableFeralGameMode = settings.Misc.EnableFeralGameMode,
                    EnableMangoHud = settings.Misc.EnableMangoHud,
                    UseDedicatedGpu = settings.Misc.UseDedicatedGpu 
                },
                Java = new JavaConfig()
                {
                    JvmArguments = settings.Java.JvmArguments,
                    JavaPath = "LAUNCH_ME_FIRST",
                    MinMemory = settings.Java.MinMemory,
                    MaxMemory = settings.Java.MaxMemory,
                    PermaGen = settings.Java.PermaGen,
                },
                Commands = new InstanceCommandsConfig(),
                EnableEnvironment = false,
                Environment = [],
                Misc =new InstanceMiscConfig()
            }
        });
        JsonHelper.WriteJsonFile(PathHelper.LauncherInstancesPath, instances);
        App.InvokeInstancesChanged();
        this.Close();
    }

    /// <summary>
    /// Handles the click event for canceling the instance creation process.
    /// Closes the current window without making any changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CancelInstance_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
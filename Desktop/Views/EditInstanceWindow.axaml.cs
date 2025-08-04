using Avalonia;
using Avalonia.Controls;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

public partial class EditInstanceWindow : Window
{
    public EditInstanceWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif
        
        
    }

    private void ModsDataGrid_OnLoading(object? sender, DataGridRowEventArgs e)
    {
        // Get the DataGridRow
        var row = e.Row;

        if (row.DataContext is not ModModel modItem)
            return;
        
        var contextMenu = new ContextMenu();
        
        // Add Enable/Disable MenuItem
        string enableDisableHeader = modItem.IsEnabled ? "Disable" : "Enable";
        var editMenuItem = new MenuItem { Header = enableDisableHeader };
        editMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(editMenuItem);
        
        // Separator
        contextMenu.Items.Add(new Separator());
        
        // Add Check For Update MenuItem
        var checkUpdateMenuItem = new MenuItem { Header = "Check for Update" };
        checkUpdateMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(checkUpdateMenuItem);
        
        // Add Change Version MenuItem
        var changeVersionMenuItem = new MenuItem { Header = "Change Version" };
        changeVersionMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(changeVersionMenuItem);
        
        // Separator
        contextMenu.Items.Add(new Separator());
        
        // Add Remove MenuItem
        var removeMenuItem = new MenuItem { Header = "Remove" };
        removeMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(removeMenuItem);
        
        // Separator
        contextMenu.Items.Add(new Separator());
        
        // Add Download Mods MenuItem
        var downloadModsMenuItem = new MenuItem { Header = "Download Mods" };
        downloadModsMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(downloadModsMenuItem);
        
        // Add Open Folder MenuItem
        var openFolderMenuItem = new MenuItem { Header = "Open Folder" };
        openFolderMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(openFolderMenuItem);

        // Assign the ContextMenu to the row
        row.ContextMenu = contextMenu;
    }

    private void ResourcePacksDataGrid_OnLoading(object? sender, DataGridRowEventArgs e)
    {
        // Get the DataGridRow
        var row = e.Row;

        if (row.DataContext is not ResourcePackModel resourcePackItem)
            return;
        
        var contextMenu = new ContextMenu();
        
        // Add Enable/Disable MenuItem
        string enableDisableHeader = resourcePackItem.IsEnabled ? "Disable" : "Enable";
        var editMenuItem = new MenuItem { Header = enableDisableHeader };
        editMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(editMenuItem);
        
        // Separator
        contextMenu.Items.Add(new Separator());
        
        // Add Remove MenuItem
        var removeMenuItem = new MenuItem { Header = "Remove" };
        removeMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(removeMenuItem);
        
        // Separator
        contextMenu.Items.Add(new Separator());
        
        // Add Download Packs MenuItem
        var downloadModsMenuItem = new MenuItem { Header = "Download Packs" };
        downloadModsMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(downloadModsMenuItem);
        
        // Add Open Folder MenuItem
        var openFolderMenuItem = new MenuItem { Header = "Open Folder" };
        openFolderMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(openFolderMenuItem);

        // Assign the ContextMenu to the row
        row.ContextMenu = contextMenu;
    }

    private void ShaderDataGrid_OnLoading(object? sender, DataGridRowEventArgs e)
    {
        // Get the DataGridRow
        var row = e.Row;

        if (row.DataContext is not ShaderPackModel shaderPackItem)
            return;
        
        var contextMenu = new ContextMenu();
        
        // Add Enable/Disable MenuItem
        string enableDisableHeader = shaderPackItem.IsEnabled ? "Disable" : "Enable";
        var editMenuItem = new MenuItem { Header = enableDisableHeader };
        editMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(editMenuItem);
        
        // Separator
        contextMenu.Items.Add(new Separator());
        
        // Add Remove MenuItem
        var removeMenuItem = new MenuItem { Header = "Remove" };
        removeMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(removeMenuItem);
        
        // Separator
        contextMenu.Items.Add(new Separator());
        
        // Add Download Packs MenuItem
        var downloadModsMenuItem = new MenuItem { Header = "Download Shaders" };
        downloadModsMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(downloadModsMenuItem);
        
        // Add Open Folder MenuItem
        var openFolderMenuItem = new MenuItem { Header = "Open Folder" };
        openFolderMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(openFolderMenuItem);

        // Assign the ContextMenu to the row
        row.ContextMenu = contextMenu;
    }

    private void WorldDataGrid_OnLoading(object? sender, DataGridRowEventArgs e)
    {
        // Get the DataGridRow
        var row = e.Row;

        if (row.DataContext is not WorldModel worldItem)
            return;
        
        var contextMenu = new ContextMenu();

        // Add Duplicate MenuItem
        var duplicateItem = new MenuItem { Header = "Duplicate" };
        duplicateItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(duplicateItem);
        
        // Add Rename MenuItem
        var renameMenuItem = new MenuItem { Header = "Rename" };
        renameMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(renameMenuItem);
        
        // Add Delete MenuItem
        var deleteMenuItem = new MenuItem { Header = "Delete" };
        deleteMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(deleteMenuItem);
        
        // Separator
        contextMenu.Items.Add(new Separator());
        
        // Add Copy Seed MenuItem
        var copySeedMenuItem = new MenuItem { Header = "Copy Seed" };
        copySeedMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(copySeedMenuItem);
        
        // Add Open Folder MenuItem
        var openFolderMenuItem = new MenuItem { Header = "Open Folder" };
        openFolderMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(openFolderMenuItem);

        // Assign the ContextMenu to the row
        row.ContextMenu = contextMenu;
    }

    private void ServerDataGrid_OnLoading(object? sender, DataGridRowEventArgs e)
    {
        // Get the DataGridRow
        var row = e.Row;

        if (row.DataContext is not ServerModel serverItem)
            return;
        
        var contextMenu = new ContextMenu();

        // Add Join MenuItem
        var joinMenuItem = new MenuItem { Header = "Join" };
        joinMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(joinMenuItem);
        
        // Add Remove MenuItem
        var removeItem = new MenuItem { Header = "Remove" };
        removeItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(removeItem);

        // Assign the ContextMenu to the row
        row.ContextMenu = contextMenu;
    }

    private void ScreenshotDataGrid_OnLoading(object? sender, DataGridRowEventArgs e)
    {
        // Get the DataGridRow
        var row = e.Row;

        if (row.DataContext is not ScreenshotModel screenshotItem)
            return;
        
        var contextMenu = new ContextMenu();

        // Add Copy MenuItem
        var copyMenuItem = new MenuItem { Header = "Copy" };
        copyMenuItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(copyMenuItem);
        
        // Add Delete MenuItem
        var deleteItem = new MenuItem { Header = "Delete" };
        deleteItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(deleteItem);
        
        // Add Rename MenuItem
        var renameItem = new MenuItem { Header = "Rename" };
        renameItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(renameItem);
        
        // Add Open Folder MenuItem
        var openFolderItem = new MenuItem { Header = "Open Folder" };
        openFolderItem.Click += (s, args) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(openFolderItem);

        // Assign the ContextMenu to the row
        row.ContextMenu = contextMenu;
    }

    private void EnvironmentTable_OnCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        // TODO
    }
}
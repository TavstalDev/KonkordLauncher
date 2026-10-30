using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Pages.CreateInstance;

public partial class ImportPage : UserControl
{
    private Button _selectedImportTypeBtn;

    public ImportPage()
    {
        InitializeComponent();
        _selectedImportTypeBtn = ImportFromFileTabBtn;
    }

    private void ImportFromFileTabBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        HandleImportTypeChange(0);
    }

    private void ImportFromUrlTabBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        HandleImportTypeChange(1);
    }

    private void HandleImportTypeChange(int index)
    {
        if (DataContext is not CreateInstanceViewModel viewModel)
            return;

        if (viewModel.Import.SelectedImportSourceIndex == index)
            return;

        viewModel.Import.SelectedImportSourceIndex = index;
        viewModel.Import.HasImportPath = false;
        viewModel.Import.ImportPath = null;
        _selectedImportTypeBtn.Classes.Remove("SuccessBtn");
        switch (index)
        {
            case 0:
            {
                _selectedImportTypeBtn = ImportFromFileTabBtn;
                break;
            }
            case 1:
            {
                _selectedImportTypeBtn = ImportFromUrlTabBtn;
                break;
            }
        }

        _selectedImportTypeBtn.Classes.Add("SuccessBtn");
    }
}


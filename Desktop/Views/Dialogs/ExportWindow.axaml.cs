using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

public partial class ExportWindow : KonkordWindow<ExportViewModel>
{
    public ExportWindow() : this(null, EInstanceProvider.Modrinth) { }
    
    public ExportWindow(Instance? instance, EInstanceProvider provider)
    {
        InitializeComponent();

        DataContext = new ExportViewModel(instance, provider);

        this.WhenActivated(disposables =>
        {
            DataContext.MinimizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState.Minimized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.MaximizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.OpenFolderPickerInteraction.RegisterHandler(async action =>
            {
                var result = await OpenFolderPickerAsync(TranslationManager.Translate("common.select.directory"));
                action.SetOutput(result);
            }).DisposeWith(disposables);
            DataContext.ShowAlertDialogInteraction.RegisterHandler(async action =>
            {
                AlertWindow alertWindow = new(action.Input.Title, action.Input.Message, action.Input.Type);
                await alertWindow.ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
        });
    }
}
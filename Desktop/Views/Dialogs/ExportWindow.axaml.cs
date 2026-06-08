using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Window used to present the export instance dialog.
/// </summary>
public partial class ExportWindow : KonkordWindow<ExportViewModel>
{
    /// <summary>
    /// Default ctor that initializes the window for preview/design use.
    /// </summary>
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public ExportWindow() : this(null, EInstanceProvider.MODRINTH) { }
    
    /// <summary>
    /// Creates a new <see cref="ExportWindow"/> bound to the provided <see cref="Instance"/> and provider.
    /// Sets up activation handlers that connect ViewModel interactions to actual UI behavior.
    /// </summary>
    /// <param name="instance">The instance to export (maybe null for design-time).</param>
    /// <param name="provider">The provider which influences export format (e.g. CurseForge or Modrinth).</param>
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public ExportWindow(Instance? instance, EInstanceProvider provider)
    {
        InitializeComponent();
        DataContext = new ExportViewModel(instance, provider);
        
        if (Design.IsDesignMode)
            return;
        
        var services = Program.ServiceProvider;
        var translationService = services.GetRequiredService<ITranslationService>();

        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.OpenFolderPickerInteraction.RegisterHandler(async action =>
            {
                var result = await OpenFolderPickerAsync(translationService.Translate("common.select.directory"));
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
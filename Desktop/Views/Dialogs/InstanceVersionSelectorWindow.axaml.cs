using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

public partial class InstanceVersionSelectorWindow : KonkordWindow<InstanceVersionSelectorViewModel>
{
    private readonly ICustomLogger _logger;
    private readonly ITranslationService _translationService;
    private readonly ILauncherStore _launcherStore;

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public InstanceVersionSelectorWindow() : this(null!) { }
    
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public InstanceVersionSelectorWindow(Instance instance)
    {
        InitializeComponent();
        DataContext = new InstanceVersionSelectorViewModel(instance);

        if (Design.IsDesignMode)
            return;

        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<InstanceVersionSelectorWindow>>();
        _translationService = services.GetRequiredService<ITranslationService>();
        _launcherStore = services.GetRequiredService<ILauncherStore>();

        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }
}
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

/// <summary>
/// Represents a window for selecting the version of an instance.
/// </summary>
public partial class InstanceVersionSelectorWindow : KonkordWindow<InstanceVersionSelectorViewModel>
{
    /// <summary>
    /// Initializes a new instance of the InstanceVersionSelectorWindow class with no parameters.
    /// </summary>
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public InstanceVersionSelectorWindow() : this(null!) { }
    
    /// <summary>
    /// Initializes a new instance of the InstanceVersionSelectorWindow class with an instance parameter.
    /// </summary>
    /// <param name="instance">The instance for which the version is being selected.</param>
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public InstanceVersionSelectorWindow(Instance instance)
    {
        InitializeComponent();
        DataContext = new InstanceVersionSelectorViewModel(instance);

        if (Design.IsDesignMode)
            return;

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
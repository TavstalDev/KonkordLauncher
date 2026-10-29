using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Services.Content;
using Tavstal.KonkordLauncher.Desktop.Services.Instances;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using Tavstal.KonkordLauncher.Desktop.Views;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation;

public class WindowFactory : IWindowFactory
{
    private readonly IServiceProvider _services;

    public WindowFactory(IServiceProvider services)
    {
        _services = services;
    }

    public MainWindow CreateMainWindow()
    {
        return _services.GetRequiredService<MainWindow>();
    }

    public CreateInstanceWindow CreateCreateInstanceWindow()
    {
        return _services.GetRequiredService<CreateInstanceWindow>();
    }

    public AccountsWindow CreateAccountsWindow()
    {
        return _services.GetRequiredService<AccountsWindow>();
    }

    public JavaSelectorWindow CreateJavaSelectorWindow()
    {
        return _services.GetRequiredService<JavaSelectorWindow>();
    }

    public IconSelectorWindow CreateIconSelectorWindow()
    {
        return _services.GetRequiredService<IconSelectorWindow>();
    }

    public ProgressWindow CreateProgressWindow(CancellationTokenSource? cancellationTokenSource = null)
    {
        return new ProgressWindow(_services.GetRequiredService<ITranslationService>(),
            _services.GetRequiredService<IBitmapService>(),
            cancellationTokenSource);
    }

    public EditInstanceWindow CreateEditInstanceWindow(InstanceModel instance)
    {
        var viewModel = new EditInstanceViewModel(instance,
            _services.GetRequiredService<IBeaconLogger<EditInstanceViewModel>>(),
            _services.GetRequiredService<ILauncherStore>(),
            _services.GetRequiredService<IEventService>(),
            _services.GetRequiredService<IBitmapService>(),
            _services.GetRequiredService<ITranslationService>(),
            _services.GetRequiredService<IEncryptionService>(),
            _services.GetRequiredService<IScreenService>(),
            _services.GetRequiredService<IDialogService>(),
            _services.GetRequiredService<IFilePickerService>());

        return new EditInstanceWindow(viewModel,
            _services.GetRequiredService<IBeaconLogger<EditInstanceWindow>>(),
            _services.GetRequiredService<IEventService>(),
            _services.GetRequiredService<IDiscordPresenceService>(),
            _services.GetRequiredService<IBitmapService>());
    }

    public InstanceLogsWindow CreateInstanceLogsWindow(string instanceId)
    {
        var viewModel = new InstanceLogsViewModel(instanceId,
            _services.GetRequiredService<IBeaconLogger<InstanceLogsViewModel>>(),
            _services.GetRequiredService<ILauncherStore>(),
            _services.GetRequiredService<IEventService>(),
            _services.GetRequiredService<IFilePickerService>());

        return new InstanceLogsWindow(viewModel, _services.GetRequiredService<IBitmapService>());
    }

    public ExportWindow CreateExportWindow(Instance instance, EInstanceProvider provider)
    {
        var viewModel = new ExportViewModel(instance, provider,
            _services.GetRequiredService<ITranslationService>(),
            _services.GetRequiredService<ModrinthPackageService>(),
            _services.GetRequiredService<CurseForgePackageService>(),
            _services.GetRequiredService<IDialogService>(),
            _services.GetRequiredService<IFilePickerService>());

        return new ExportWindow(viewModel, _services.GetRequiredService<IBitmapService>());
    }

    public InstanceVersionSelectorWindow CreateInstanceVersionSelectorWindow(Instance instance)
    {
        var viewModel = new InstanceVersionSelectorViewModel(instance,
            _services.GetRequiredService<IBeaconLogger<InstanceVersionSelectorViewModel>>(),
            _services.GetRequiredService<ILauncherStore>(),
            _services.GetRequiredService<IManifestService>(),
            _services.GetRequiredService<IEventService>());

        return new InstanceVersionSelectorWindow(viewModel, _services.GetRequiredService<IBitmapService>());
    }

    public ResourceDownloadWindow CreateResourceDownloadWindow(Instance instance, EResourceType resourceType)
    {
        var viewModel = new ResourceDownloadViewModel(instance, resourceType,
            _services.GetRequiredService<IBeaconLogger<ResourceDownloadViewModel>>(),
            _services.GetRequiredService<ITranslationService>(),
            _services.GetRequiredService<ILauncherStore>(),
            _services.GetRequiredService<IManifestService>(),
            _services.GetRequiredService<IMetaCacheService>(),
            _services.GetRequiredService<IBitmapService>(),
            _services.GetRequiredService<IDialogService>());

        return new ResourceDownloadWindow(viewModel, _services.GetRequiredService<IBitmapService>());
    }

    public ResourceReviewWindow CreateResourceReviewWindow(Instance instance, EResourceType resourceType,
        List<ResourceDownloadModel> resources)
    {
        return new ResourceReviewWindow(instance, resourceType, resources,
            _services.GetRequiredService<IHttpService>(),
            _services.GetRequiredService<ITranslationService>(),
            _services.GetRequiredService<ILauncherStore>(),
            _services.GetRequiredService<IMetaCacheService>(),
            this,
            _services.GetRequiredService<IDialogService>(),
            _services.GetRequiredService<IBitmapService>());
    }
}

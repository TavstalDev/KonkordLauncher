using Tavstal.BeaconSDK.Models.Config;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Services.Instances;

public class InstanceModelFactory : IInstanceModelFactory
{
    private readonly BeaconConfiguration _beaconConfiguration;
    private readonly IBitmapService _bitmapService;
    private readonly IDialogService _dialogService;
    private readonly IEncryptionService _encryptionService;
    private readonly IEventService _eventService;
    private readonly IInstanceInstallService _installService;
    private readonly IJavaService _javaService;
    private readonly IInstanceLaunchService _launchService;
    private readonly ILauncherStore _launcherStore;
    private readonly IBeaconLogger<InstanceModel> _logger;
    private readonly IManifestService _manifestService;
    private readonly IScreenService _screenService;
    private readonly ITranslationService _translationService;
    private readonly IWindowFactory _windowFactory;

    public InstanceModelFactory(
        IBeaconLogger<InstanceModel> logger,
        ILauncherStore launcherStore,
        IManifestService manifestService,
        IBitmapService bitmapService,
        ITranslationService translationService,
        IJavaService javaService,
        IInstanceInstallService installService,
        IInstanceLaunchService launchService,
        IEventService eventService,
        IEncryptionService encryptionService,
        BeaconConfiguration beaconConfiguration,
        IScreenService screenService,
        IWindowFactory windowFactory,
        IDialogService dialogService)
    {
        _logger = logger;
        _launcherStore = launcherStore;
        _manifestService = manifestService;
        _bitmapService = bitmapService;
        _translationService = translationService;
        _javaService = javaService;
        _installService = installService;
        _launchService = launchService;
        _eventService = eventService;
        _encryptionService = encryptionService;
        _beaconConfiguration = beaconConfiguration;
        _screenService = screenService;
        _windowFactory = windowFactory;
        _dialogService = dialogService;
    }

    public InstanceModel Create(Instance instance)
    {
        return new InstanceModel(instance, _logger, _launcherStore, _manifestService, _bitmapService,
            _translationService, _javaService, _installService, _launchService, _eventService,
            _encryptionService, _beaconConfiguration, _screenService, _windowFactory, _dialogService);
    }
}

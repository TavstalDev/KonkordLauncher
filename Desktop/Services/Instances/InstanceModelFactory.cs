using Tavstal.BeaconSDK.Models.Config;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Services.Instances;

/// <inheritdoc />
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

    /// <summary>
    ///     Creates a new instance of the <see cref="InstanceModelFactory" /> class.
    /// </summary>
    /// <param name="logger">The logger for instance models.</param>
    /// <param name="launcherStore">The store for launcher data.</param>
    /// <param name="manifestService">The service that loads manifests.</param>
    /// <param name="bitmapService">The service that loads bitmaps.</param>
    /// <param name="translationService">The service that translates text.</param>
    /// <param name="javaService">The service that manages Java runtimes.</param>
    /// <param name="installService">The service that installs instance files.</param>
    /// <param name="launchService">The service that starts the game.</param>
    /// <param name="eventService">The service that shares launcher events.</param>
    /// <param name="encryptionService">The service that encrypts and decrypts data.</param>
    /// <param name="beaconConfiguration">The Beacon SDK configuration.</param>
    /// <param name="screenService">The service that reports the screen size.</param>
    /// <param name="windowFactory">The factory that creates windows.</param>
    /// <param name="dialogService">The service that shows dialogs.</param>
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

    /// <inheritdoc />
    public InstanceModel Create(Instance instance)
    {
        return new InstanceModel(instance, _logger, _launcherStore, _manifestService, _bitmapService,
            _translationService, _javaService, _installService, _launchService, _eventService,
            _encryptionService, _beaconConfiguration, _screenService, _windowFactory, _dialogService);
    }
}

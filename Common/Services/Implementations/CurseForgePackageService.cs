using Microsoft.Extensions.Logging;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Package;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Common.Services.Implementations;

public class CurseForgePackageService : IPackageService
{
    private readonly ILogger _logger;
    private readonly IHttpService _httpService;
    private readonly ILauncherStore _launcherStore;
    
    public CurseForgePackageService(ILogger<CurseForgePackageService> logger, IHttpService httpService, ILauncherStore launcherStore)
    {
        _logger = logger;
        _httpService = httpService;
        _launcherStore = launcherStore;
    }
    
    /// <inheritdoc/>
    public async Task<Instance?> ImportAsync(string sourcePath, Resolution resolution, string? customName = null, string? customGroup = null,
        string? customIconUrl = null, IProgressReporter? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to import curse forge package: {ex}");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExportAsync(Instance instance, List<FileNode> fileNodes, string targetPath, string exportVersion = "1.0.0",
        string summary = "", IProgressReporter? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to export curse forge package: {ex}");
            return false;
        }
    }
}
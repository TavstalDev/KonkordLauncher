using Tavstal.KonkordLauncher.Common.Models;

namespace Tavstal.KonkordLauncher.Common.Helpers;

public static class InstanceHelper
{
    public static async Task ImportAsync(string sourcePath, EInstanceProvider provider, CancellationToken cancellationToken = default)
    {
        switch (provider)
        {
            case EInstanceProvider.Konkord:
            {
                await ImportKonkordAsync(sourcePath, cancellationToken);
                break;
            }
            case EInstanceProvider.PrismLauncher:
            {
                await ImportPrismLauncherAsync(sourcePath, cancellationToken);
                break;
            }
            case EInstanceProvider.Modrinth:
            {
                await ImportModrinthAsync(sourcePath, cancellationToken);
                break;
            }
            case EInstanceProvider.CurseForge:
            {
                await ImportCurseForgeAsync(sourcePath, cancellationToken);
                break;
            }
        }
    }

    public static async Task ExportAsync(Instance instance, string targetPath, EInstanceProvider provider, CancellationToken cancellationToken = default)
    {
        switch (provider)
        {
            case EInstanceProvider.Konkord:
            {
                await ExportKonkordAsync(instance, targetPath, cancellationToken);
                break;
            }
            case EInstanceProvider.PrismLauncher:
            {
                await ExportPrismLauncherAsync(instance, targetPath, cancellationToken);
                break;
            }
            case EInstanceProvider.Modrinth:
            {
                await ExportModrinthAsync(instance, targetPath, cancellationToken);
                break;
            }
            case EInstanceProvider.CurseForge:
            {
                await ExportCurseForgeAsync(instance, targetPath, cancellationToken);
                break;
            }
        }
    }

    #region Konkord

    private static async Task ImportKonkordAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        
    }
    
    private static async Task ExportKonkordAsync(Instance instance, string targetPath, CancellationToken cancellationToken = default)
    {
        
    }

    #endregion

    #region PrismLauncher

    private static async Task ImportPrismLauncherAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        
    }
    
    private static async Task ExportPrismLauncherAsync(Instance instance, string targetPath, CancellationToken cancellationToken = default)
    {
        
    }

    #endregion
    
    #region Modrinth

    private static async Task ImportModrinthAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        
    }
    
    private static async Task ExportModrinthAsync(Instance instance, string targetPath, CancellationToken cancellationToken = default)
    {
        
    }

    #endregion
    
    #region CurseForge

    private static async Task ImportCurseForgeAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        
    }
    
    private static async Task ExportCurseForgeAsync(Instance instance, string targetPath, CancellationToken cancellationToken = default)
    {
        
    }

    #endregion
}
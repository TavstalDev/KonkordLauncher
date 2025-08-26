using Tavstal.KonkordLauncher.Common.Models;

namespace Tavstal.KonkordLauncher.Common.Helpers;

public static class InstanceHelper
{
    public static async Task ImportAsync(string sourcePath, EInstanceProvider provider)
    {
        switch (provider)
        {
            case EInstanceProvider.Konkord:
            {
                await ImportKonkordAsync(sourcePath);
                break;
            }
            case EInstanceProvider.PrismLauncher:
            {
                await ImportPrismLauncherAsync(sourcePath);
                break;
            }
            case EInstanceProvider.Modrinth:
            {
                await ImportModrinthAsync(sourcePath);
                break;
            }
            case EInstanceProvider.CurseForge:
            {
                await ImportCurseForgeAsync(sourcePath);
                break;
            }
        }
    }

    public static async Task ExportAsync(Instance instance, string targetPath, EInstanceProvider provider)
    {
        switch (provider)
        {
            case EInstanceProvider.Konkord:
            {
                await ExportKonkordAsync(instance, targetPath);
                break;
            }
            case EInstanceProvider.PrismLauncher:
            {
                await ExportPrismLauncherAsync(instance, targetPath);
                break;
            }
            case EInstanceProvider.Modrinth:
            {
                await ExportModrinthAsync(instance, targetPath);
                break;
            }
            case EInstanceProvider.CurseForge:
            {
                await ExportCurseForgeAsync(instance, targetPath);
                break;
            }
        }
    }

    #region Konkord

    private static async Task ImportKonkordAsync(string sourcePath)
    {
        
    }
    
    private static async Task ExportKonkordAsync(Instance instance, string targetPath)
    {
        
    }

    #endregion

    #region PrismLauncher

    private static async Task ImportPrismLauncherAsync(string sourcePath)
    {
        
    }
    
    private static async Task ExportPrismLauncherAsync(Instance instance, string targetPath)
    {
        
    }

    #endregion
    
    #region Modrinth

    private static async Task ImportModrinthAsync(string sourcePath)
    {
        
    }
    
    private static async Task ExportModrinthAsync(Instance instance, string targetPath)
    {
        
    }

    #endregion
    
    #region CurseForge

    private static async Task ImportCurseForgeAsync(string sourcePath)
    {
        
    }
    
    private static async Task ExportCurseForgeAsync(Instance instance, string targetPath)
    {
        
    }

    #endregion
}
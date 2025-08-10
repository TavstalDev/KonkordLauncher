namespace Tavstal.KonkordLauncher.Core.Models.Endpoints.Platforms;

public static class CurseForgeEndpoints
{
    private const string _baseUrl = "https://api.curseforge.com";
    public const int GameId = 432; // Minecraft game ID
    
    // Example https://api.curseforge.com/v1/categories?gameId=432
    public const string GetCategories = _baseUrl + "/v1/categories?gameId=432";
}
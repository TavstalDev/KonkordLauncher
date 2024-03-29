using KonkordLibrary.Models.Fabric;
using KonkordLibrary.Models.Minecraft.Library;
using Newtonsoft.Json.Linq;

namespace KonkordLibrary.Models.GameManager
{
    public class LibraryResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string AssetIndex { get; set; }
        public string ClientDownloadUrl { get; set; }
        public string LibrarySizeCachePath { get; set; }
        public int LocalLibrarySize { get; set; }
        public List<MCLibrary> Libraries {  get; set; }
        public string CustomGameMain {  get; set; }
        public List<string> GameArgs { get; set; }
        public List<string> JavaArgs { get; set; }

        public LibraryResponse() { }

        public LibraryResponse(bool isSuccess, string message, string assetIndex, string clientDownloadUrl, string librarySizeCachePath, int localLibrarySize, List<MCLibrary> libraries)
        {
            IsSuccess = isSuccess;
            Message = message;
            AssetIndex = assetIndex;
            ClientDownloadUrl = clientDownloadUrl;
            LibrarySizeCachePath = librarySizeCachePath;
            LocalLibrarySize = localLibrarySize;
            Libraries = libraries;
        }
    }
}

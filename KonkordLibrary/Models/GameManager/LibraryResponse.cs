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
        public JArray Libraries {  get; set; }

        public LibraryResponse() { }

        public LibraryResponse(bool isSuccess, string message, string assetIndex, string clientDownloadUrl, string librarySizeCachePath, JArray libraries)
        {
            IsSuccess = isSuccess;
            Message = message;
            AssetIndex = assetIndex;
            ClientDownloadUrl = clientDownloadUrl;
            LibrarySizeCachePath = librarySizeCachePath;
            Libraries = libraries;
        }
    }
}

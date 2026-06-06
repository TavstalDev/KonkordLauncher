namespace Tavstal.KonkordLauncher.Core.Models;

/// <summary>
/// Represents a file download entry with a source URL, destination path, and optional progress reporting.
/// </summary>
public class DownloadEntry
{
    /// <summary>
    /// Gets or sets the source URL of the file to download.
    /// </summary>
    public string Url { get; set; }
    
    /// <summary>
    /// Gets or sets the local file path where the downloaded file will be saved.
    /// </summary>
    public string Path { get; set; }
    
    /// <summary>
    /// Gets or sets the progress reporter for the download operation.
    /// </summary>
    public IProgress<double>? Progress { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadEntry"/> class.
    /// </summary>
    public DownloadEntry() {}
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadEntry"/> class with a URL, path, and optional progress reporter.
    /// </summary>
    /// <param name="url">The source URL of the file to download.</param>
    /// <param name="path">The local file path to save the download to.</param>
    /// <param name="_progressReporter">An optional progress reporter that reports download progress and status updates.</param>
    public DownloadEntry(string url, string path, IProgressReporter? _progressReporter = null)
    {
        Url = url;
        Path = path;
        if (_progressReporter != null)
        {
            string fileName = System.IO.Path.GetFileName(path);
            Progress = new Progress<double>(p =>
            {
                _progressReporter.ReportProgress(p);
                _progressReporter.UpdateStatusTranslated("instance.download.file", fileName, p.ToString("0.00"));
            });
        }
    }
}
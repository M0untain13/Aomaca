namespace AomacaCore.Services.GoogleDownloaderService;

public class GoogleDownloaderService : IGoogleDownloaderService
{
    private FileDownloader? _downloader;

    public void DownloadFile(string address, string fileName)
    {
        _downloader ??= new FileDownloader();
        _downloader.DownloadFile(address, fileName);
        _downloader.Dispose();
        _downloader = null;
    }
}
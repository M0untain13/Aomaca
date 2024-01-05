namespace AomacaCore.Services.GoogleDownloaderService;

public interface IGoogleDownloaderService
{
    void DownloadFile(string address, string fileName);
}
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using System.Net.NetworkInformation;
using AomacaCore.Services.GoogleDownloaderService;

namespace AomacaCore.ViewModels;



public class StartViewModel : MvxViewModel
{
    private readonly IMvxNavigationService _navigationService;
    private readonly IGoogleDownloaderService _downloaderService;

    #region Текстовое поле

    private string _text = "";
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    #endregion

    private string _errorText = "";

    private bool
        _isChecked,
        _isDownloaded,
        _isError;

    private bool CheckInternetConnection()
    {
        try
        {
            var myPing = new Ping();
            var host = "google.com";
            var buffer = new byte[32];
            var timeout = 1000;
            var pingOptions = new PingOptions();
            var reply = myPing.Send(host, timeout, buffer, pingOptions);
            return (reply.Status == IPStatus.Success);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void Start()
    {
        Task.Run(() =>
        {
            // TODO: мне кажется, надо вынести все пути в файл конфигурации
            _isDownloaded = true;
            var distDir = new DirectoryInfo(@"PyScripts\main.dist");
            if (distDir.Exists)
            {
                var scriptFile = new FileInfo(@"PyScripts\main.dist\main.exe");
                if (scriptFile.Exists)
                    _isDownloaded = false;
            }

            _isChecked = false;
            if (_isDownloaded)
            {
                if (CheckInternetConnection())
                {
                    _downloaderService.DownloadFile("https://drive.google.com/file/d/1DbWW5-9tupImSurE2pkpcSKK-x9BHaJl/view?usp=sharing", @"PyScripts\main.dist.zip");

                    System.IO.Compression.ZipFile.ExtractToDirectory(
                        @"PyScripts\main.dist.zip",
                        "PyScripts");
                }
                else
                {
                    _errorText = "Необходимо подключение к интернету.";
                    _isError = true;
                }
                _isDownloaded = false;
            }
        });

        Task.Run(() =>
        {
            while (_isChecked)
            {
                Text = "Подождите. Проверка целостности скриптов";
                for (var i = 0; i < 3; i++)
                {
                    Text += '.';
                    Thread.Sleep(400);
                }
            }

            while (_isDownloaded)
            {
                Text = "Файлы не найдены. Попытка скачать";
                for (var i = 0; i < 3; i++)
                {
                    Text += '.';
                    Thread.Sleep(400);
                }
            }

            if (_isError)
            {
                Text = "Ошибка! ";
                Text += _errorText;
            }
            else
                _navigationService.Navigate<MainViewModel>();
        });
    }

    public StartViewModel(IMvxNavigationService navigationService, IGoogleDownloaderService downloaderService)
    {
        _navigationService = navigationService;
        _downloaderService = downloaderService;
        _isChecked = true;
        Start();
    }
}
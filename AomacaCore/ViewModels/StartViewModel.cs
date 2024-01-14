using System.Net;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using System.Net.NetworkInformation;
using System.IO.Compression;
using System.Text;

namespace AomacaCore.ViewModels;

// TODO: мне кажется это надо убрать вообще, ну или сделать проверку целостности, но ничего не качать

public class StartViewModel : MvxViewModel
{
    private readonly IMvxNavigationService _navigationService;

    #region Текстовые поля

    private string _checkText = "";
    public string CheckText
    {
        get => _checkText;
        set => SetProperty(ref _checkText, value);
    }
    
    private string _errorText = "";
    public string ErrorText
    {
        get => _errorText;
        set => SetProperty(ref _errorText, value);
    }

    #endregion

    // Где лежат скрипты
    private const string scriptDir = "PyScripts";

    private bool _isChecked;

    private bool IsScriptExists()
    {
        var distDir = new DirectoryInfo(@$"{scriptDir}");
        if (!distDir.Exists)
            return false;
        var scriptFile = new FileInfo(@$"{scriptDir}\main.exe");
        if (!scriptFile.Exists)
            return false;
        return true;
    }

    private void StartCheck()
    {
        _isChecked = true;
        Task.Run(() =>
        {
            while (_isChecked)
            {
                CheckText = "Подождите. Проверка целостности скриптов";
                for (var i = 0; i < 3; i++)
                {
                    if (!_isChecked)
                        break;
                    CheckText += '.';
                    Thread.Sleep(400);
                }
            }
            CheckText = "";
        });

        Task.Run(() =>
        {
            var isFileExists = IsScriptExists();
            _isChecked = false;
            if (isFileExists)
            {
                _navigationService.Navigate<MainViewModel>();
            }
            else
            {
                ErrorText = "Ошибка! Файл(ы) поврежден(ы) или отсутствует(ют).";
            }
        });
    }

    public StartViewModel(IMvxNavigationService navigationService)
    {
        _navigationService = navigationService;
        _isChecked = true;
        StartCheck();
    }
}
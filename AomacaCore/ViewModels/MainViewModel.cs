using AomacaCore.Models;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using AomacaCore.Services.AnalyzerService;
using MvvmCross.Navigation;

namespace AomacaCore.ViewModels;

//TODO: Кстати, ты потом прошерсти весь код на шарпе и питоне, чтобы все по красоте было.
//		(форматирование и проверка на возникновение ошибок)

//TODO: Надо разделить скрипты. Файл с нейросеткой выполняется слишком долго.

//TODO: Добавить установщик

//TODO: не забудь потом добавить ссылку, откуда взял ELA/EXIF скрипты

//TODO: нужно будет добавить функцию сохранения результатов

//TODO: следует вычислить контрольные суммы, когда буду установщик архивировать и кидать на яндекс или гугл диск.

//TODO: привести интерфейс к нормальному виду

//TODO: надо создать питоновский файл для обучения нейросети

//TODO: надо протестировать ml.net c .onnx моделью

public class MainViewModel : MvxViewModel
{
    private readonly IAnalyzerService _analyzerService;

    #region Пути

    private Paths _paths;

	public string PathToOriginal
	{
		get => _paths.pathToOriginal;
		set => SetProperty(ref _paths.pathToOriginal, value);
    }

    public string PathToResavedOrig
    {
        get => _paths.pathToResavedOrig;
        set => SetProperty(ref _paths.pathToResavedOrig, value);
    }

	public string PathToEla
	{
		get => _paths.pathToEla;
		private set => SetProperty(ref _paths.pathToEla, value);
	}

    #endregion

    #region Текстовые поля

    private TextFields _textFields;

    public string MetadataText
    {
        get => _textFields.metadata;
        private set => SetProperty(ref _textFields.metadata, value);
    }

    public string ExifAnalysisResult
    {
        get => _textFields.exifAnalysis;
        private set => SetProperty(ref _textFields.exifAnalysis, value);
    }

    public string ElaAnalysisResult
    {
        get => _textFields.elaCnnAnalysis;
        private set => SetProperty(ref _textFields.elaCnnAnalysis, value);
    }

    public string FinalAnalysisResult
    {
        get => _textFields.finalAnalysis;
        private set => SetProperty(ref _textFields.finalAnalysis, value);
    }

    #endregion

    #region Результаты

    private Results _results;

    #endregion

    #region Статус-бар

    private string _statusText = string.Empty;
	public string StatusText
	{
		get => _statusText;
		set
		{
			if (SetProperty(ref _statusText, value) && value != "")
			{
				_timer = 4;
			}
		}
	}

	private ushort _timer;

    #endregion

    private bool
        _isSignal,
        _isCancel;

    private readonly string _scriptDir = "PyScripts";

    public void AnalysisStart()
    {
        _isSignal = true;
    }

    public void AnalysisCancel()
    {
        _isCancel = true;
        _isSignal = true;
    }

    public IMvxAsyncCommand AnalysisAsyncCommand { get; }

    public MainViewModel(IAnalyzerService analyzerService, IMvxNavigationService navigationService)
	{
        _analyzerService = analyzerService;

        #region Таймер для отчистки статус-бара

        Task.Run(() =>
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (_timer > 0)
                    _timer--;
                if (_timer == 0)
                    StatusText = "";
            }
        });

        #endregion

        #region Инициализация команд

        AnalysisAsyncCommand = new MvxAsyncCommand(() =>
        {
            return Task.Run(() =>
            {
                while (!_isSignal) { Thread.Sleep(250); }
                _isSignal = false;

                if (_isCancel)
                {
                    _isCancel = false;
                }
                else
                {
                    StartAnalysis();
                }
            });
        });

        #endregion
    }

    private void ClearFilesDir()
    {
        var dirInfo = new DirectoryInfo("Files");
        if (!dirInfo.Exists)
        {
            dirInfo.Create();
        }
        foreach (var file in dirInfo.GetFiles())
        {
            file.Delete();
        }
    }

    private void StartAnalysis()
    {
        ClearFields();
        ClearFilesDir();

        var statusRunning = true;
        var statusTask = new Task(() =>
        {
            while (statusRunning)
            {
                StatusText = "Подождите. Изображение анализируется";
                for (var i = 0; i < 3; i++)
                {
                    if (!statusRunning)
                        break;
                    StatusText += '.';
                    Thread.Sleep(400);
                }
            }
        });

        statusTask.Start();
        ExifAnalysis();
        ElaAnalysis();
        CnnAnalysis();
        Conclusion();

        statusRunning = false;
        statusTask.Wait();
        StatusText = "Анализ завершён!";
    }

    private void ElaAnalysis()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var result = _analyzerService.ElaMethod(PathToOriginal).Split();
        var nameResavedOrig = result[0];
        var nameEla = result[1];

        PathToResavedOrig = $@"{currentDir}\{nameResavedOrig}";
        PathToEla = $@"{currentDir}\{nameEla}";
    }

    private void ExifAnalysis()
    {
        var exifResult = _analyzerService.ExifMethod(PathToOriginal).Split("||");
        
        MetadataText = exifResult[0];
        ExifAnalysisResult = exifResult[1];
        if (ExifAnalysisResult.Contains("Обнаружена программа для редактирования фото") || ExifAnalysisResult.Contains("Дата создания и дата изменения не совпадают"))
            _results.metadataFeaturesDetected = true;
    }

    private void CnnAnalysis()
    {
        /*
        var result = _analyzerService.NeuralNetworkMethod(PathToOriginal);
        _results.cnnAnswer = Convert.ToDecimal(result.Replace('.', ','));
        //ElaAnalysisResult = $"Нейросеть считает, что это изображение могло быть подделано с шансом {result}%.";
        */
        ElaAnalysisResult = "Анализ нейросети не настроен.";
    }

    private void Conclusion()
    {
        if (_results.cnnAnswer > 70 || _results.metadataFeaturesDetected)
        {
            FinalAnalysisResult = "Вывод: изображение было подделано.";
        }
        else
        {
            FinalAnalysisResult = "Вывод: изображение оригинальное.";
        }
    }

    private void ClearFields()
    {
        _results.metadataFeaturesDetected = false;
        _results.cnnAnswer = 0;
        PathToResavedOrig = PathToEla = MetadataText = ExifAnalysisResult = ElaAnalysisResult = FinalAnalysisResult
            = string.Empty;
    }
		
}
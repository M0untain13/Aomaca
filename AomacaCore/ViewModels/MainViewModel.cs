using MvvmCross.Commands;
using MvvmCross.ViewModels;
using AomacaCore.Services.AnalyzerService;
using static System.Net.Mime.MediaTypeNames;

namespace AomacaCore.ViewModels;

//TODO: Кстати, ты потом прошерсти весь код на шарпе и питоне, чтобы все по красоте было.
//		(форматирование и проверка на возникновение ошибок)

//TODO: При запуске приложения, отсутствующий скрипт может скачаться с инета, но я думаю, нужно сделать установщик, который вместе с приложением установит и скрипты

//TODO: не забудь потом добавить ссылку, откуда взял ELA/EXIF скрипты и FileDownloader

//TODO: нужно будет добавить функцию сохранения результатов

// TODO: возможно в питоновских скриптах нужно закрывать файлы в конце

public class MainViewModel : MvxViewModel
{
    private readonly IAnalyzerService _analyzerService;

    #region Данные

    #region Путь до исходного изображения

    private string _pathToOriginal = string.Empty;
	public string PathToOriginal
	{
		get => _pathToOriginal;
		set => SetProperty(ref _pathToOriginal, value);
    }

    #endregion

    #region Путь до сохраннёного оригинала

    private string _pathToResavedOrig = string.Empty;
    public string PathToResavedOrig
    {
        get => _pathToResavedOrig;
        set => SetProperty(ref _pathToResavedOrig, value);
    }
    /*
    private MemoryStream _resOrig;

    public MemoryStream ResOrig
    {
        get => _resOrig;
        set => SetProperty(ref _resOrig, value);
    }
    */
    #endregion

    #region Путь до ELA изображения

    private string _pathToEla = string.Empty;
	public string PathToEla
	{
		get => _pathToEla;
		private set => SetProperty(ref _pathToEla, value);
	}

	#endregion

	#region Метаданные

	private string _metadataText = string.Empty;
	public string MetadataText
	{
		get => _metadataText;
		private set => SetProperty(ref _metadataText, value);
	}

	#endregion

	#region Анализ

	private string _exifAnalysisResult = string.Empty;
	public string ExifAnalysisResult
	{
		get => _exifAnalysisResult;
		private set => SetProperty(ref _exifAnalysisResult, value);
	}

	private string _elaAnalysisResult = string.Empty;
	public string ElaAnalysisResult
	{
		get => _elaAnalysisResult;
		private set => SetProperty(ref _elaAnalysisResult, value);
	}

	private decimal _fakeChance = 0.00M;

	private string _finalAnalysisResult = string.Empty;
	public string FinalAnalysisResult
	{
		get => _finalAnalysisResult;
		private set => SetProperty(ref _finalAnalysisResult, value);
	}

	#endregion

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

    public MainViewModel(IAnalyzerService analyzerService)
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
                    ClearFields();

                    var dirInfo = new DirectoryInfo("Files");
                    if (!dirInfo.Exists)
                    {
                        dirInfo.Create();
                    }
                    foreach (var file in dirInfo.GetFiles())
                    {
                        file.Delete();
                    }

                    var statusRunning = true;
                    var statusTask = new Task(() =>
                    {
                        while (statusRunning)
                        {
                            StatusText = "Подождите. Изображение анализируеся";
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
            });
        });

        #endregion
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
        var exifPath = _analyzerService.ExifMethod(PathToOriginal);
        var sr = new StreamReader(exifPath);
        var line = sr.ReadLine();
        var metadata = new Dictionary<string, string>();
        var metadataKeys = new[] { "Software", "DateTimeOriginal", "DateTime" };
        while (line != null)
        {
            var pair = line.Split("||");
            metadata[pair[0]] = pair[1];
            line = sr.ReadLine();
        }
        sr.Close();

        foreach (var pair in metadata)
        {
            if (metadataKeys.Contains(pair.Key))
            {
                if (MetadataText != "")
                    MetadataText += '\n';
                MetadataText += pair.Value;
            }
            else
            {
                if (ExifAnalysisResult != "")
                    ExifAnalysisResult += '\n';
                ExifAnalysisResult += pair.Value;
            }
        }
        if (ExifAnalysisResult == "")
            ExifAnalysisResult = "В метаданных признаки не обнаружены.";
    }

    private void CnnAnalysis()
    {
        _fakeChance = Convert.ToDecimal(_analyzerService.NeuralNetworkMethod(PathToEla));
        ElaAnalysisResult = $"Нейросеть считает, что это изображение могло быть подделано с шансом {_fakeChance}%.";
    }

    private void Conclusion()
    {
        if (_fakeChance > 70)
        {
            FinalAnalysisResult = "Вывод: Изображение было подделано.";
        }
        else
        {
            FinalAnalysisResult = "N/A";
        }
    }

    private void ClearFields() => 
		PathToResavedOrig = PathToEla = MetadataText = ExifAnalysisResult = ElaAnalysisResult = FinalAnalysisResult 
            = string.Empty;
}
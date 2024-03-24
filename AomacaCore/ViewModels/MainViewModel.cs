using AomacaCore.Models;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using AomacaCore.Services.AnalyzerService;
using MvvmCross.Navigation;
using System.Runtime.CompilerServices;

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

//TODO: реализуй сохранение результатов в виде текстового файла и отдельно в виде архива с изображениями

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
		_isCancel,
		_isZipMode;

	public bool IsDone { get; private set; }

	private readonly string _scriptDir = "PyFiles";

	private string[] _imagePaths = Array.Empty<string>();

	public void AnalysisStart(string[] paths)
	{
		_imagePaths = paths;
		if (paths.Length > 1)
			_isZipMode = true;
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
				IsDone = false;

				while (!_isSignal)
					Thread.Sleep(250);
				_isSignal = false;


                if (!_isCancel)
                {
                    foreach (var path in _imagePaths)
                    {
                        PathToOriginal = path;
                        StartAnalysis();
                        // TODO: тут нужно сделать сохранение в архив, если активирован ZipMode
                    }
                }


                IsDone = true;
				_isCancel = false;

				if (_isZipMode)
				{
					ClearFields();
					ClearFilesDir();
                    PathToOriginal = "";
                    var dirPath = Path.GetDirectoryName(_imagePaths[0]);
					// TODO: тут почему-то иногда запрещается удаление временной папки
                    Directory.Delete(dirPath, true);
                }
			});
		});

		#endregion
	}

    private void ClearFields()
    {
        _results.metadataFeaturesDetected = false;
        _results.cnnAnswer = 0;
        PathToResavedOrig = PathToEla = MetadataText = ExifAnalysisResult = ElaAnalysisResult = FinalAnalysisResult
            = string.Empty;
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
		IsDone = false;
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
		IsDone = true;
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
		var detected = exifResult[2];

		if (detected == "1")
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
}
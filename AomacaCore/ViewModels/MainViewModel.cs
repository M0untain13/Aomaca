using AomacaCore.Models;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using AomacaCore.Services.AnalyzerService;
using MvvmCross.Navigation;
using System.Runtime.CompilerServices;
using AomacaCore.Services.SavingService;

namespace AomacaCore.ViewModels;

//TODO: Кстати, ты потом прошерсти весь код на шарпе и питоне, чтобы все по красоте было.
//		(форматирование и проверка на возникновение ошибок)

//TODO: Добавить установщик

//TODO: следует вычислить контрольные суммы, когда буду установщик архивировать и кидать на яндекс или гугл диск.

//TODO: надо создать питоновский файл для обучения нейросети

//TODO: надо протестировать ml.net c .onnx моделью

//TODO: метаданные в формате .tiff не вытягиваются тем методом на питоне

//TODO: не забудь сделать форматирование даты в exif.py

public class MainViewModel : MvxViewModel
{
	private readonly IAnalyzerService _analyzerService;

	private readonly ISavingService _savingService;

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

	public bool IsDone { get; private set; }

	private readonly string _scriptDir = "PyFiles";

	private string[] _imagePaths = Array.Empty<string>();

	public void AnalysisStart(string[] paths)
	{
		_imagePaths = paths;
        _isSignal = true;
	}

	public void AnalysisCancel()
	{
		_isCancel = true;
		_isSignal = true;
	}

	public IMvxAsyncCommand AnalysisAsyncCommand { get; }

	public IMvxAsyncCommand<string> SaveAsyncCommand { get; }

	public IMvxAsyncCommand OpenResultsInExplorerCommand { get; }

	public MainViewModel(IAnalyzerService analyzerService, ISavingService savingService)
	{
		_analyzerService = analyzerService;
        _savingService = savingService;

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

		OpenResultsInExplorerCommand = new MvxAsyncCommand(() =>
		{
			return Task.Run(() =>
			{
                var path = @$"{System.IO.Path.GetDirectoryName(Environment.ProcessPath)}\SavedResults"; 
                
				var arguments = $"/open, \"{path}\"";

                System.Diagnostics.Process.Start("explorer.exe", arguments);
            });
		});

		SaveAsyncCommand = new MvxAsyncCommand<string>(selectedSaving =>
		{
			return Task.Run(() =>
			{
				if (!IsDone || PathToOriginal == "")
					return;

				var texts = new[] { _paths.pathToOriginal, _textFields.metadata, _textFields.exifAnalysis, _textFields.elaCnnAnalysis, _textFields.finalAnalysis };
				switch (selectedSaving)
				{
					case "All":
						StatusText = "Будут сохранены изображения и текст.";
						// TODO: С путем к оригинальному изображению нужно будет быть по-аккуратнее, т.к. приложение не удерживает изображение, т.е. его могут удалить или переместить
						var paths = new[] { _paths.pathToOriginal, _paths.pathToEla };
						_savingService.Zip(new[] { _savingService.Save(texts, paths) });
						StatusText = "Изображения и текст сохранены!";
						break;
					case "TextOnly":
						StatusText = "Будет сохранен только текст.";
						_savingService.Zip(new[] { _savingService.Save(texts) });
						StatusText = "Текст сохранен!";
						break;
				}
			});
		});

        AnalysisAsyncCommand = new MvxAsyncCommand(() =>
		{
			return Task.Run(() =>
            {
                IsDone = false;

                while (!_isSignal)
                    Thread.Sleep(250);
                _isSignal = false;

                var isZipMode = _imagePaths.Length > 1;

                var pathsForZip = new List<string>();

                if (!_isCancel)
                {
                    foreach (var path in _imagePaths)
                    {
                        PathToOriginal = path;
                        StartAnalysis();

                        if (isZipMode)
                        {
                            var texts = new[] { _paths.pathToOriginal, _textFields.metadata, _textFields.exifAnalysis, _textFields.elaCnnAnalysis, _textFields.finalAnalysis };
                            var paths = new[] { _paths.pathToOriginal, _paths.pathToEla };
							pathsForZip.Add(_savingService.Save(texts, paths));
                        }
                    }
                }

                if (isZipMode)
				{
                    _savingService.Zip(pathsForZip.ToArray());
                    pathsForZip.Clear();
                }

                IsDone = true;
                _isCancel = false;

                if (isZipMode)
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
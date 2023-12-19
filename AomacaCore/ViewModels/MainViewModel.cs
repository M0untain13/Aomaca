using AomacaCore.Services;
using MvvmCross.Commands;
using MvvmCross.ViewModels;

namespace AomacaCore.ViewModels;

//TODO: Кстати, ты потом прошерсти весь код на шарпе и питоне, чтобы все по красоте было.
//		(форматирование и проверка на возникновение ошибок)

//TODO: При сборке необходимо, чтобы как-то PyScripts попадал в папку сборки, иначе анализ работать не будет

//TODO: не забудь потом добавить ссылку, откуда взял ELA скрипт
public class MainViewModel : MvxViewModel
{
	#region Данные

	#region Путь до исходного изображения

	private string _pathToOriginal = string.Empty;
	public string PathToOriginal
	{
		get => _pathToOriginal;
		set
		{
			if (SetProperty(ref _pathToOriginal, value))
			{
				ClearFields();
                if (value != "")
					_isStartAnalysis = true;
            }
		}
	}

	private bool _isStartAnalysis;

    #endregion

    #region Путь до сохраннёного оригинала

    private string _pathToResavedOrig = string.Empty;
    public string PathToResavedOrig
    {
        get => _pathToResavedOrig;
        set => SetProperty(ref _pathToResavedOrig, value);
    }

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

	private string _dateCreate = string.Empty;
	public string DateCreate
	{
		get => _dateCreate;
		private set => SetProperty(ref _dateCreate, value);
	}

	private string _dateEdit = string.Empty;
	public string DateEdit
	{
		get => _dateEdit;
		private set => SetProperty(ref _dateEdit, value);
	}

	private string _device = string.Empty;
	public string Device
	{
		get => _device;
		private set => SetProperty(ref _device, value);
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

	public IMvxAsyncCommand AnalysisAsyncCommand { get; }

    public MainViewModel(IAnalyzerService analyzerService)
	{
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
                #region Фоновые задачи

                var exifAnalysisTask = new Task(() =>
                {
                    Thread.Sleep(1000);
					var path = analyzerService.ExifMethod(PathToOriginal);

                    //TODO: Пока что будем верить, что файл нормальный и не имеет ошибок.
                    
                    var sr = new StreamReader(path);
                    var line = sr.ReadLine();
					var metadata = new Dictionary<string, string>();
                    while (line != null)
                    {
                        var pair = line.Split("||");
						metadata[pair[0]] = pair[1];
                        line = sr.ReadLine();
                    }
					sr.Close();
                    ExifAnalysisResult = string.Join('\n', metadata.Values);
                    StatusText = "Анализ EXIF завершил свою работу.";
                });

                var elaAnalysisTask = new Task(() =>
                {
                    Thread.Sleep(500);
					var currentDir = System.IO.Directory.GetCurrentDirectory();

                    var (nameResavedOrig, nameEla) = analyzerService.ElaMethod(PathToOriginal);
					PathToResavedOrig = $@"{currentDir}\{nameResavedOrig}";
					PathToEla = $@"{currentDir}\{nameEla}";

                    _fakeChance = analyzerService.NeuralNetworkMethod(PathToEla);

                    ElaAnalysisResult = $"Нейросеть считает, что это изображение могло быть подделано с шансом {_fakeChance}%.";
                    StatusText = "Анализ ELA завершил свою работу.";
                });

                var finalAnalysisTask = new Task(() =>
                {
                    exifAnalysisTask.Wait();
                    elaAnalysisTask.Wait();
                    Thread.Sleep(1000);

					// TODO: сделать нормальную реализацию

					if(_fakeChance > 70)
					{
                        FinalAnalysisResult = "Вывод: Изображение было подделано.";
                    }
					else
					{
                        FinalAnalysisResult = "N/A";
                    }

                    StatusText = "Анализ завершён.";
                });

                #endregion

				// TODO: может ли быть такая ситуация, что анализ никогда не начнется?
                while (!_isStartAnalysis) { }

                exifAnalysisTask.Start();
                elaAnalysisTask.Start();
                finalAnalysisTask.Start();

                finalAnalysisTask.Wait();
                _isStartAnalysis = false;
            });
        });

        #endregion
    }

	private void ClearFields() => 
		PathToResavedOrig = PathToEla = DateCreate = DateEdit = Device = ExifAnalysisResult = ElaAnalysisResult = FinalAnalysisResult = string.Empty;
}
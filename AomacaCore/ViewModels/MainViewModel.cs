using MvvmCross.Commands;
using MvvmCross.ViewModels;
using AomacaCore.Services.AnalyzerService;

namespace AomacaCore.ViewModels;

//TODO: Кстати, ты потом прошерсти весь код на шарпе и питоне, чтобы все по красоте было.
//		(форматирование и проверка на возникновение ошибок)

//TODO: При запуске приложения, отсутствующий скрипт может скачаться с инета, но я думаю, нужно сделать устновщик, который вместе с приложением установит и скрипты

//TODO: не забудь потом добавить ссылку, откуда взял ELA/EXIF скрипты и FileDownloader

//TODO: нужно будет добавить функцию сохранения результатов

// TODO: возможно в питоновских скриптах нужно закрывать файлы в конце

// BUG: приложение держит в заложниках изображения до момента закрытия приложения

public class MainViewModel : MvxViewModel
{
	#region Данные

	#region Путь до исходного изображения

	private string _pathToOriginal = string.Empty;
	public string PathToOriginal
	{
		get => _pathToOriginal;
		set => SetProperty(ref _pathToOriginal, value);
    }

    public bool 
        isSignal,
        isCancel;

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
                ClearFields();


                var elaTask = new Task(() =>
                {
                    var currentDir = Directory.GetCurrentDirectory();
                    var result = analyzerService.ElaMethod(PathToOriginal).Split();
                    var nameResavedOrig = result[0];
                    var nameEla = result[1];
                    PathToResavedOrig = $@"{currentDir}\{nameResavedOrig}";
                    PathToEla = $@"{currentDir}\{nameEla}";
                });

                var exifTask = new Task(() =>
				{
					elaTask.Wait();

                    var exifPath = analyzerService.ExifMethod(PathToOriginal);
                    var sr = new StreamReader(exifPath);
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

                var cnnTask = new Task(() =>
                {
					elaTask.Wait();

                    _fakeChance = Convert.ToDecimal(analyzerService.NeuralNetworkMethod(PathToEla));
                    ElaAnalysisResult = $"Нейросеть считает, что это изображение могло быть подделано с шансом {_fakeChance}%.";
                    StatusText = "Анализ ELA завершил свою работу.";
                });

                var endTask = new Task(() =>
                {
					exifTask.Wait();
					cnnTask.Wait();

                    if (_fakeChance > 70)
                    {
                        FinalAnalysisResult = "Вывод: Изображение было подделано.";
                    }
                    else
                    {
                        FinalAnalysisResult = "N/A";
                    }
                    StatusText = "Анализ завершён.";
                });

                while (!isSignal) { Thread.Sleep(250); }
                isSignal = false;

                if (isCancel)
                {
                    isCancel = false;
                }
                else
                {
                    var dirInfo = new DirectoryInfo("Files");
                    if (!dirInfo.Exists)
                    {
                        dirInfo.Create();
                    }
                    foreach (var file in dirInfo.GetFiles())
                    {
                        while(true)
                            try
                            {
                                file.Delete();
                                break;
                            }
                            catch
                            {
                                Thread.Sleep(200);
                            }
                    }

                    elaTask.Start();
                    exifTask.Start();
                    cnnTask.Start();
                    endTask.Start();

                    endTask.Wait();
                }
            });
        });

        #endregion
    }

	private void ClearFields() => 
		PathToResavedOrig = PathToEla = DateCreate = DateEdit = Device = ExifAnalysisResult = ElaAnalysisResult = FinalAnalysisResult 
            = string.Empty;
}
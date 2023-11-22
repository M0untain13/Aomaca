using AomacaCore.Services;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MvvmCross.Commands;
using MvvmCross.ViewModels;

namespace AomacaCore.ViewModels;

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

	public IMvxAsyncCommand AnalysisAsynCommand { get; }

    public MainViewModel()
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

        AnalysisAsynCommand = new MvxAsyncCommand(() =>
        {
            return Task.Run(() =>
            {
                #region Фоновые задачи

                var exifAnalysisTask = new Task(() =>
                {
                    Thread.Sleep(1000);
                    _Analysis();
                    ExifAnalysisResult = "N/A";
                    StatusText = "Анализ EXIF завершил свою работу.";
                });

                var elaAnalysisTask = new Task(() =>
                {
                    Thread.Sleep(3000);
                    ElaAnalysisResult = "N/A";
                    StatusText = "Анализ ELA завершил свою работу.";
                });

                var finalAnalysisTask = new Task(() =>
                {
                    exifAnalysisTask.Wait();
                    elaAnalysisTask.Wait();
                    Thread.Sleep(1000);
                    FinalAnalysisResult = "N/A";
                    StatusText = "Анализ завершён.";
                });

                #endregion


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

	private void _Analysis()
	{
		// TODO: теги неправильно читаются
		var directories = ImageMetadataReader.ReadMetadata(PathToOriginal);
		{
			var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
			{
				var dateTimeOriginal = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);
				{
					DateCreate = $"Дата создания: {dateTimeOriginal ?? "Отсутствует"}";
				}
				var dateTimeDigitized = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeDigitized);
				{
					DateEdit = $"Дата изменения {dateTimeDigitized ?? "Отсутствует"}";
				}
				var device = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDeviceSettingDescription);
				{
					Device = $"Устройство: {device ?? "Отсутствует"}";
				}
			}
		}
	}

	private void ClearFields()
	{
		PathToEla = "";
		DateCreate = "";
		DateEdit = "";
		Device = "";
		ExifAnalysisResult = "";
		ElaAnalysisResult = "";
		FinalAnalysisResult = "";
	}
}
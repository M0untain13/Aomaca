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
				if(value != "")
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

	#region Задачи анализа

	private Task _exifAnalysisTask;

	private Task _elaAnalysisTask;

	private Task _finalAnalysisTask;

	#endregion

	#region Статус-бар

	private string _statusText;
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

	private ushort _timer = 0;

	private Task _timerTask;

    #endregion

	public IMvxAsyncCommand AnalysisAsynCommand { get; }

    public MainViewModel()
	{
		_exifAnalysisTask = new Task(() =>
		{
            Thread.Sleep(1000);
            _Analysis();
            ExifAnalysisResult = "N/A";
			StatusText = "Анализ EXIF завершил свою работу.";
        });

		_elaAnalysisTask = new Task(() =>
		{
			Thread.Sleep(3000);
            ElaAnalysisResult = "N/A";
            StatusText = "Анализ ELA завершил свою работу.";
        });

		_finalAnalysisTask = new Task(() =>
		{
			Thread.Sleep(1000);
            FinalAnalysisResult = "N/A";
            StatusText = "Анализ завершён.";
        });

        _timerTask = new Task(() =>
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

        AnalysisAsynCommand = new MvxAsyncCommand(() =>
        {
            return Task.Run(() =>
            {
                while (!_isStartAnalysis) { }

                _exifAnalysisTask.Start();
                _elaAnalysisTask.Start();
                _exifAnalysisTask.Wait();
                _elaAnalysisTask.Wait();

                _finalAnalysisTask.Start();
                _finalAnalysisTask.Wait();
                _isStartAnalysis = false;
            });
        });
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

	private void ClearAllFields()
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
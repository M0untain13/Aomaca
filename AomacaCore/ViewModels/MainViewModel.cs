using AomacaCore.Services;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MvvmCross.ViewModels;

namespace AomacaCore.ViewModels;

public class MainViewModel : MvxViewModel
{
	#region Сервисы

	private INeuralNetworkService _neuralNetworkService;

	#endregion

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
				Task.Run(_Analysis);
			}
		}
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

	private string _analisys = string.Empty;
	public string Analysis
	{
		get => _analisys;
		private set => SetProperty(ref _analisys, value);
	}

	#endregion

	#endregion

	public MainViewModel(INeuralNetworkService nnService)
	{
		_neuralNetworkService = nnService;
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
}
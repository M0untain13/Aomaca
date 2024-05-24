using AomacaCore.ViewModels;
using Microsoft.Win32;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AomacaApp.Views;

[MvxViewFor(typeof(MainViewModel))]
public partial class MainView : MvxWpfView
{
	public MainView() => InitializeComponent();

	private const string _aboutApp =
        "Приложение Aomaca.\n\n" +
        "Данное приложение может быть использовано в целях выявления поддельных изображений.\n\n";

	private const string _functional =
        "Чтобы начать анализ, нажмите на кнопку <<Открыть...>>. Затем выберите изображение формата .jpg или архив .zip с изображениями формата .jpg. " +
        "После выбора файла, анализ начнётся автоматически. В зависимости от ресурсов устройства, количества и размера файлов, анализ может длиться продолжительное время.\n\n" +
        "Чтобы сохранить результат, нажмите на кнопку <<Сохранить...>> и выберите способ сохранения.\n\n" +
        "Чтобы посмотреть сохранённые результаты, нажмите на кнопку <<Сохранённые результаты>>.";

    private const string _helpText = 
		"=== О приложении ===\n" + 
		_aboutApp + 
		"=== Функционал ===\n" + 
		_functional;

    private void HelpButtonClick(object sender, System.Windows.RoutedEventArgs e) => MessageBox.Show(_helpText, "Помощь");

	private void OpenFileButtonClick(object sender, RoutedEventArgs e)
	{
		if (DataContext is not MainViewModel viewModel) 
			return;

		var openFileDialog = new OpenFileDialog();
		var result = openFileDialog.ShowDialog() ?? false;
		// TODO: тут нужно проверить, открыт файл или архив
		if (result)
		{
			var extension = Path.GetExtension(openFileDialog.FileName);

			Task.Run(() =>
			{
				switch (extension)
				{
					case ".jpg":
						viewModel.AnalysisStart(new[] { openFileDialog.FileName });

						while (!viewModel.IsDone)
							Thread.Sleep(250);

						break;
					case ".zip":
						var zipFilePath = openFileDialog.FileName;
						var extractFolderPath = Path.GetDirectoryName(zipFilePath) + "\\TempUnzip_" + DateTime.Now.ToString("HH-mm-ss");
						Directory.CreateDirectory(extractFolderPath);
                        ZipFile.ExtractToDirectory(zipFilePath, extractFolderPath);
						var filesPathList = Directory.GetFiles(extractFolderPath, "*", SearchOption.AllDirectories);

						var list = new List<string>();
						foreach(var path in filesPathList)
						{
                            var ext = Path.GetExtension(path);
							if(ext == ".jpg")
							{
								list.Add(path);
							}
                        }

						if(list.Count > 0)
						{
                            viewModel.AnalysisStart(list.ToArray());
                        }
						else
						{
                            viewModel.AnalysisCancel();
                            viewModel.StatusText = "Архив не имеет изображений формата .jpg!";
                        }

						while(!viewModel.IsDone)
							Thread.Sleep(250);

						break;
					default:
                        viewModel.AnalysisCancel();
						viewModel.StatusText = "Формат файла должен быть .jpg или .zip!";

                        break;
                }
			});
		}
		else
		{
			viewModel.AnalysisCancel();
		}
		
		
	}

	private void Orig_MouseUp_Open(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		if (DataContext is MainViewModel viewModel && viewModel.PathToResavedOrig != "")
			Process.Start("explorer.exe", viewModel.PathToResavedOrig);
	}

	private void Ela_MouseUp_Open(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		if (DataContext is MainViewModel viewModel && viewModel.PathToEla != "")
			Process.Start("explorer.exe", viewModel.PathToEla);
	}
}
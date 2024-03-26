using AomacaCore.ViewModels;
using Microsoft.Win32;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AomacaApp.Views;

[MvxViewFor(typeof(MainViewModel))]
public partial class MainView : MvxWpfView
{
	public MainView() => InitializeComponent();

	private void HelpButtonClick(object sender, System.Windows.RoutedEventArgs e) => MessageBox.Show("Тут должна быть написана инструкция.", "Помощь");

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
						viewModel.AnalysisStart(filesPathList);

						while(!viewModel.IsDone)
							Thread.Sleep(250);

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
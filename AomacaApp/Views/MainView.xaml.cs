using AomacaCore.ViewModels;
using Microsoft.Win32;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using System.Diagnostics;
using System.Windows;

namespace AomacaApp.Views;

[MvxViewFor(typeof(MainViewModel))]
public partial class MainView : MvxWpfView
{
    public MainView() => InitializeComponent();

    private void HelpButtonClick(object sender, System.Windows.RoutedEventArgs e) => MessageBox.Show("Тут должна быть написана инструкция.", "Помощь");

    private void OpenFileButtonClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog();

        if (DataContext is MainViewModel viewModel)
        {
            var result = openFileDialog.ShowDialog() ?? false;
            if (result)
                viewModel.PathToOriginal = openFileDialog.FileName;
            else
                viewModel.isCancel = true;
            viewModel.isSignal = true;
        }
            
    }

    private void ResavedOrig_MouseUp_Open(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
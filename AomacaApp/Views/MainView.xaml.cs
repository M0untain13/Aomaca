using AomacaCore.ViewModels;
using Microsoft.Win32;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
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

        if (!(openFileDialog.ShowDialog() ?? false)) 
            return;

        if (DataContext is MainViewModel viewModel)
            viewModel.PathToOriginal = openFileDialog.FileName;        
    }
}
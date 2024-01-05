using AomacaCore.ViewModels;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using System.Windows;

namespace AomacaApp.Views;

[MvxViewFor(typeof(StartViewModel))]
public partial class StartView : MvxWpfView
{
    public StartView() => InitializeComponent();

    private void HelpButtonClick(object sender, System.Windows.RoutedEventArgs e) => MessageBox.Show("Тут должна быть написана инструкция.", "Помощь");
}
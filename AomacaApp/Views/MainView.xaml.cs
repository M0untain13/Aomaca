using AomacaCore.ViewModels;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;

namespace AomacaApp.Views;

[MvxViewFor(typeof(MainViewModel))]
public partial class MainView : MvxWpfView
{
    public MainView() => InitializeComponent();
}
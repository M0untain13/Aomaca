using AomacaCore.ViewModels;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;

namespace AomacaApp.Views;

[MvxViewFor(typeof(StartViewModel))]
public partial class StartView : MvxWpfView
{
    public StartView() => InitializeComponent();
}
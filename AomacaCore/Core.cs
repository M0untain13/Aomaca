using AomacaCore.ViewModels;
using MvvmCross.IoC;
using MvvmCross.ViewModels;

namespace AomacaCore;

public class Core : MvxApplication
{
    public override void Initialize()
    {
        CreatableTypes()
            .EndingWith("Service")
            .AsInterfaces()
            .RegisterAsLazySingleton();

        RegisterAppStart<StartViewModel>();
    }
}
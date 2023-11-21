using Microsoft.Extensions.Logging;
using MvvmCross.Platforms.Wpf.Core;

namespace AomacaApp;

public class Setup : MvxWpfSetup<AomacaCore.Core>
{
    protected override ILoggerProvider? CreateLogProvider() => null;
    protected override ILoggerFactory? CreateLogFactory() => null;
}
using System.Diagnostics;

namespace AomacaCore.Services.AnalyzerService;


// TODO: все публичные методы должны возвращать одну строку


public class AnalyzerService : IAnalyzerService
{
	public string ExifMethod(string path)
	{
        RunCmd($"exif \"{path}\"");

        return @$"{filesDir}\exif_result.txt";
	}

	public (string, string) ElaMethod(string path)
	{
        RunCmd( $"ela \"{path}\" 25");

        return (@$"{filesDir}\resaved_image.jpg", @$"{filesDir}\ela_image.png");
	}

	public decimal NeuralNetworkMethod(string path)
	{
		// TODO: убрать заглушку и реализовать метод
		return 95.23M;
	}

    // Директория, куда будут сохранятся файлы
    private const string filesDir = "Files";

    private void RunCmd(string args)
	{
        var info = new ProcessStartInfo
        {
            // TODO: мне кажется, надо вынести все пути в файл конфигурации
            FileName = @"PyScripts\main.dist\main.exe",
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true, 
            RedirectStandardError = true
        };
        using var process = Process.Start(info);
        process?.WaitForExit();

        using var reader = process?.StandardError;
        var result = reader?.ReadToEnd();
    }
}
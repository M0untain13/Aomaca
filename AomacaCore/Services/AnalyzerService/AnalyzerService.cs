using System.Diagnostics;

namespace AomacaCore.Services.AnalyzerService;


public class AnalyzerService : IAnalyzerService
{
	public string ExifMethod(string path)
	{
        RunCmd($"exif \"{path}\"");

        return @$"{filesDir}\exif_result.txt";
	}

	public string ElaMethod(string path)
	{
        RunCmd( $"ela \"{path}\" 100");

        return @$"{filesDir}\resaved_image.jpg {filesDir}\ela_image.png";
	}

	public string NeuralNetworkMethod(string path)
	{
		RunCmd($"cnn \"{path}\"");

		return $@"{filesDir}\cnn_result.txt";
	}

    // Где будут сохранятся файлы
    private const string filesDir = "Files";
    // Где лежат скрипты
    private const string scriptDir = "PyScripts";

    private void RunCmd(string args)
	{
        var info = new ProcessStartInfo
        {
            // TODO: мне кажется, надо вынести все пути в файл конфигурации
            FileName = @$"{scriptDir}\main.exe",
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
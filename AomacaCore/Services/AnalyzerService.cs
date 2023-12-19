using System.Diagnostics;

namespace AomacaCore.Services;

public class AnalyzerService : IAnalyzerService
{
	public string ExifMethod(string path)
	{
        // Путь до скрипта
        const string pathToScript = @$"{baseDir}\exif.py";

        RunCmd(pathToScript, pathToPython, $"{path}");

        return @$"{filesDir}\exif_result.txt";
	}

	public (string, string) ElaMethod(string path)
	{
		// Путь до скрипта
		const string pathToScript = @$"{baseDir}\ela.py";
		// Качество выходного изображения
		const int quality = 25;

		RunCmd(pathToScript, pathToPython, $"{path} {quality}");

		return (@$"{filesDir}\resaved_image.jpg", @$"{filesDir}\ela_image.png");
	}

	public decimal NeuralNetworkMethod(string path)
	{
		// TODO: убрать заглушку и реализовать метод
		return 95.23M;
	}

    // Директория, где лежат скрипты и вирт.пространство
    private const string baseDir = "PyScripts";
    // Директория, куда будут сохранятся файлы
    private const string filesDir = "Files";
    // Путь до python.exe
    private const string pathToPython = @$"{baseDir}\.venv\Scripts\python.exe";

    private void RunCmd(string pathToScr, string pathToPy, string args)
	{
		var start = new ProcessStartInfo
		{
			FileName = pathToPy,
			Arguments = $"{pathToScr} {args}",
			UseShellExecute = false,
			RedirectStandardOutput = true,
            CreateNoWindow = true
        };
		using var process = Process.Start(start);
		process?.WaitForExit();
	}
}
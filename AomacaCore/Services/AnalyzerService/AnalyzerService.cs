using System.Diagnostics;
using System.Text;

namespace AomacaCore.Services.AnalyzerService;


public class AnalyzerService : IAnalyzerService
{
	public string ExifMethod(string path)
	{
        // TODO: надо переделать метод
		var lines = RunCmd("exif.exe", $"\"{path}\"").Split('\n');
		var metadata = new Dictionary<string, string>();
		foreach (var line in lines)
		{
			var pair = line.Split("||");
			if (pair.Length == 2)
				metadata[pair[0]] = pair[1];
		}

		var metadataText = "";
		var exifAnalysisResult = "";
		var metadataKeys = new[] { "Software", "DateTimeOriginal", "DateTime" };

		foreach (var pair in metadata)
		{
			if (metadataKeys.Contains(pair.Key))
			{
				if (metadataText != "")
					metadataText += '\n';
				metadataText += pair.Value;
			}
			else
			{
				if (exifAnalysisResult != "")
					exifAnalysisResult += '\n';
				exifAnalysisResult += pair.Value;
			}
		}
		if (exifAnalysisResult == "")
			exifAnalysisResult = "В метаданных признаки не обнаружены.";

		return string.Concat(metadataText, "||", exifAnalysisResult);
	}

	public string ElaMethod(string path)
	{
		RunCmd("ela.exe", $"\"{path}\" 100");

		return @$"{filesDir}\resaved_image.jpg {filesDir}\ela_image.png";
	}

	public string NeuralNetworkMethod(string path)
	{
		var result = RunCmd(@"cnn\cnn.exe", $"\"{path}\"");

        return result;
	}

	// Где будут сохранятся файлы
	private const string filesDir = "Files";
	// Где лежат скрипты
	private const string scriptDir = "PyFiles";

	private string RunCmd(string scriptPath, string args)
	{
		var info = new ProcessStartInfo
		{
			// TODO: мне кажется, надо вынести все пути в файл конфигурации
			FileName = @$"{scriptDir}\{scriptPath}",
			Arguments = args,
			UseShellExecute = false,
			CreateNoWindow = true,
			//StandardOutputEncoding = Encoding.Default,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
		using var process = Process.Start(info);
		process?.WaitForExit();

		// Это для дебаггинга TODO: потом убрать
		using var errReader = process?.StandardError;
		var error = errReader?.ReadToEnd();

		using var outReader = process?.StandardOutput;
		var result = outReader?.ReadToEnd() ?? "";
		return result;
	}
}
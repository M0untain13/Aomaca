using System.Diagnostics;
using System.Text;

namespace AomacaCore.Services.AnalyzerService;


public class AnalyzerService : IAnalyzerService
{
	public string ExifMethod(string path)
	{
		// TODO: надо переделать метод
		var lines = RunCmd("exif.exe", $"\"{path}\"").Split('\n');

		var metadataText = "";
		var analysisText = "";
		var detected = "0";
		var result = "";

		var dictOfMetadataTypes = new Dictionary<string, string>
		{
			{"Software", "ПО"},
            {"DateTimeOriginal", "Дата создания"},
            {"DateTime", "Дата изменения"}
        };

		if(lines.Length > 0) 
		{
			if (lines[0].Split("||")[0] != "Error")
			{
                foreach (var line in lines)
                {
                    var data = line.Replace("\r", "").Split("||");
                    switch (data.Length)
                    {
                        case 2:
                        {
                            if (metadataText != "")
                                metadataText += "\n";

                            var type = dictOfMetadataTypes[data[0]];
                            var text = data[1];
                            metadataText += $"{type}: {text}";
                            break;
                        }
                        case 3:
                        {
                            if (analysisText != "")
                                analysisText += "\n";

                            analysisText += data[1];

                            if (detected != "1" && data[2] == "1")
                                detected = "1";
                            break;
                        }
                    }
                }
                result = string.Concat(metadataText, "||", analysisText, "||", detected);
            }
        }

		if (result == "")
			result = "||Метаданные не обнаружены.||0";

		return result;
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
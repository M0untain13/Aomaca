using System.Diagnostics;

namespace AomacaCore.Services.AnalyzerService;


public class AnalyzerService : IAnalyzerService
{
	public string ExifMethod(string path)
	{
        RunCmd($"exif \"{path}\"");

        var resultPath = @$"{filesDir}\exif_result.txt";
        var metadataText = "";
        var exifAnalysisResult = "";

        var sr = new StreamReader(resultPath);
        var line = sr.ReadLine();
        var metadata = new Dictionary<string, string>();
        var metadataKeys = new[] { "Software", "DateTimeOriginal", "DateTime" };
        while (line != null)
        {
            var pair = line.Split("||");
            metadata[pair[0]] = pair[1];
            line = sr.ReadLine();
        }
        sr.Close();

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
        RunCmd( $"ela \"{path}\" 100");

        return @$"{filesDir}\resaved_image.jpg {filesDir}\ela_image.png";
	}

	public string NeuralNetworkMethod(string path)
	{
		RunCmd($"cnn \"{path}\"");

        var resultPath = $@"{filesDir}\cnn_result.txt";
        var sr = new StreamReader(resultPath);
        // TODO: возможно стоит обработать ошибку другим способом
        var result = sr.ReadLine() ?? throw new NullReferenceException();
        sr.Close();

        return result;
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

        // Это для дебаггинга TODO: потом убрать
        using var reader = process?.StandardError;
        var result = reader?.ReadToEnd();
    }
}
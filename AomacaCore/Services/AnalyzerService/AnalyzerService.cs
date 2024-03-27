using System.Diagnostics;
using Microsoft.ML;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
		double[] data;

		using (var image = Image.Load<Rgb24>(path))
		{
            image.Mutate(x => x.Resize(128, 128));
			var pixelArray = new Rgb24[image.Width * image.Height];
            image.CopyPixelDataTo(pixelArray);
			data = new double[pixelArray.Length * 3];
            for (var i = 0; i < pixelArray.Length; i++)
			{
				data[i*3] = pixelArray[i].R;
				data[i*3 + 1] = pixelArray[i].G;
				data[i*3 + 2] = pixelArray[i].B;
            }
        }

        for (var i = 0; i < data.Length; i++)
        {
			data[i] /= 255;
        }

		// TODO: оказывается в итоге должна получится матрица 1x128x128x3
		// То есть корневой массив содержит один элементв в виде массива размером 128, который в каждой ячейке содержит еще 128, которые содержат 3 элемента RGB в диапазоне от 0 до 1 

        var mlContext = new MLContext();

		// TODO: при вызове метода вызывается исключение
        var estimator = mlContext.Transforms.ApplyOnnxModel(@$"{scriptDir}\trained_model.onnx");

        //IDataView dataView = mlContext.Data.LoadFromEnumerable();

        //estimator.Fit()

        return "";
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
using System.Diagnostics;
using Microsoft.ML;
using Microsoft.ML.Data;
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

    private class Input
    {
        [VectorType(1 * 128 * 128 * 3)] // Our model takes a three-dimensional tensor as input but ML.NET takes a flatten vector as input
        [ColumnName("conv2d_6_input")] // This name must match the input node's name
        public float[] Data { get; set; }
    }

    private class Output
    {
        [VectorType(1)] // Because output node has (1) shape
        [ColumnName("dense_5")] // This name must match the output node's name
        public float[] Data { get; set; }
    }

    public float NeuralNetworkMethod(string path)
	{
        var data = new float[128*128*3];

		using (var image = Image.Load<Rgb24>(path))
		{
			image.Mutate(x => x.Resize(128, 128));
			var pixelArray = new Rgb24[image.Width * image.Height];
			image.CopyPixelDataTo(pixelArray);
			for (var i = 0; i < pixelArray.Length; i++)
			{
				data[i * 3]     = (float)pixelArray[i].R / 255;
                data[i * 3 + 1] = (float)pixelArray[i].G / 255;
                data[i * 3 + 2] = (float)pixelArray[i].B / 255;
            }
		}
        var mlContext = new MLContext();
        var estimator = mlContext.Transforms.ApplyOnnxModel("dense_5", "conv2d_6_input", @$"{scriptDir}\trained_model.onnx");

        var input = new Input [] { new Input { Data = data } };
        var dataView = mlContext.Data.LoadFromEnumerable(input);
        var transformedData = estimator.Fit(dataView).Transform(dataView);
        var output = mlContext.Data.CreateEnumerable<Output>(transformedData, reuseRowObject: false).ToList();

        return 1 - output[0].Data[0];
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
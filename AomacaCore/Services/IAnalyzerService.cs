namespace AomacaCore.Services;

public interface IAnalyzerService
{
	string ExifMethod(string path);

	(string, string) ElaMethod(string path);

	decimal NeuralNetworkMethod(string path);
}
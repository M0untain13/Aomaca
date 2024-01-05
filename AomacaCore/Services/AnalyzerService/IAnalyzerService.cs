namespace AomacaCore.Services.AnalyzerService;

public interface IAnalyzerService
{
	string ExifMethod(string path);

	string ElaMethod(string path);

	string NeuralNetworkMethod(string path);
}
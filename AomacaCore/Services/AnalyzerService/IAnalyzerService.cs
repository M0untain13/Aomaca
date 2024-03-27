namespace AomacaCore.Services.AnalyzerService;

public interface IAnalyzerService
{
	string ExifMethod(string path);

	string ElaMethod(string path);

	float NeuralNetworkMethod(string path);
}
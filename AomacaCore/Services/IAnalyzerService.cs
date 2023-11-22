namespace AomacaCore.Services;

public interface IAnalyzerService
{
    (string, string, string, string) ExifMethod(string path);

    string ElaMethod(string path);

    decimal NeuralNetworkMethod(string path);
}
namespace AomacaCore.Models;

public struct Results
{
    public decimal cnnAnswer;
    public bool metadataFeaturesDetected; // TODO: нужно отметить обнаружение признаков редактирования в метаданных

    public Results()
    {
        cnnAnswer = 0.00M;
        metadataFeaturesDetected = false;
    }
}
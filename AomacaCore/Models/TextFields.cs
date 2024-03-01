namespace AomacaCore.Models;

public struct TextFields
{
    public string
        metadata,
        exifAnalysis,
        elaCnnAnalysis,
        finalAnalysis;

    public TextFields() => metadata = exifAnalysis = elaCnnAnalysis = finalAnalysis = "";
}
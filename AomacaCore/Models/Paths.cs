namespace AomacaCore.Models;

public struct Paths
{
    public string 
        pathToOriginal,
        pathToResavedOrig,
        pathToEla;

    public Paths() => pathToOriginal = pathToResavedOrig = pathToEla = "";
}
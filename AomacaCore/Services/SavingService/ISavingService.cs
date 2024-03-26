namespace AomacaCore.Services.SavingService;

public interface ISavingService
{
    void Save(string[] texts, string[]? paths = null);
}
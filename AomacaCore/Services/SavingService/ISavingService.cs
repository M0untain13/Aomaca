namespace AomacaCore.Services.SavingService;

public interface ISavingService
{
    string Save(string[] texts, string[]? paths = null);

    void Zip(string[] paths);
}
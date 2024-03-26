namespace AomacaCore.Services.SavingService;

public class SavingService : ISavingService
{
    private string _savingDir = "SavedResults";
    static SavingService()
    {
        // TODO: тут нужно указать, в какую папку будут сохраняться все результаты
    }

    // TODO: реализовать логику сохранения результата анализа
    public void Save(string[] texts, string[]? paths = null)
    {
        if (paths is null)
        {

        }
        else
        {

        }
    }
}
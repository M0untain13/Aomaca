using System.IO;
using System.IO.Compression;

namespace AomacaCore.Services.SavingService;

public class SavingService : ISavingService
{
    private const string _DIR_TO_SAVE = "SavedResults";
    static SavingService()
    {
        if (!Directory.Exists(_DIR_TO_SAVE))
        {
            Directory.CreateDirectory(_DIR_TO_SAVE);
        }
    }

    // TODO: реализовать логику сохранения результата анализа
    public string Save(string[] texts, string[]? paths = null)
    {
        var folderName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var dirPath = @$"{_DIR_TO_SAVE}\{folderName}";
        Directory.CreateDirectory(dirPath);

        using (var fs = new FileStream(dirPath + "\\result.txt", FileMode.Create))
        {
            using (var writer = new StreamWriter(fs))
            {
                foreach(var text in texts)
                {
                    if (text != "")
                        writer.WriteLine(text);
                }
            }
        }

        if (paths is not null)
        {
            foreach(var oldPath in paths)
            {
                var newPath = @$"{dirPath}\{Path.GetFileName(oldPath)}"; 
                File.Copy(oldPath, newPath, true);
            }
        }

        return dirPath;
    }

    public void Zip(string[] paths)
    {
        var zipName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var zipPath = @$"{_DIR_TO_SAVE}\{zipName}.zip";

        using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            foreach (string path in paths)
            {
                var directoryInfo = new DirectoryInfo(path);
                foreach (var file in directoryInfo.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    var entryName = Path.GetFileName(path) + "\\" + file.FullName.Substring(directoryInfo.FullName.Length + 1);
                    archive.CreateEntryFromFile(file.FullName, entryName);
                }
                Directory.Delete(path, true);
            }
        }

        File.SetAttributes(zipPath, FileAttributes.ReadOnly);
    }
}
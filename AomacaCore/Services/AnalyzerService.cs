using MetadataExtractor.Formats.Exif;
using MetadataExtractor;
using System.Xml.XPath;

namespace AomacaCore.Services;

public class AnalyzerService : IAnalyzerService
{
    public (string, string, string, string) ExifMethod(string path)
    {
        string 
            dataCreate = "",
            dateEdit = "",
            device = "",
            result = "";

        var directories = ImageMetadataReader.ReadMetadata(path);
        {
            var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            {
                var dto = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);
                {
                    dataCreate = $"Дата создания: {dto ?? "Отсутствует"}";
                }
                var dtd = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeDigitized);
                {
                    dateEdit = $"Дата изменения {dtd ?? "Отсутствует"}";
                }
                var d = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDeviceSettingDescription);
                {
                    device = $"Устройство: {d ?? "Отсутствует"}";
                }
            }
        }

        // TODO: убрать заглушку и сделать номарльную реализацию
        result = "В метаданных обнаружено...";

        return (dataCreate, dateEdit, device, result);
    }

    public string ElaMethod(string path)
    {
        // TODO: убрать заглушку и реализовать метод
        return path;
    }

    public decimal NeuralNetworkMethod(string path)
    {
        // TODO: убрать заглушку и реализовать метод
        return 95.23M;
    }
}
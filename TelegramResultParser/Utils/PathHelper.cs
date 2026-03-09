namespace TelegramResultParser.Utils;

public class PathHelper
{
    public static string[] ResolveJsonPath(string path)
    {
        if(string.IsNullOrEmpty(path))
            return Array.Empty<string>();

        if (Directory.Exists(path))
        {
            return Directory.GetFiles(path, "*.json", searchOption: SearchOption.AllDirectories);
        }

        if (File.Exists(path))
        {
            return Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase) ? new []{path} : Array.Empty<string>();
        }
        return Array.Empty<string>();
    }
    public static string MakeSafeFileName(string name, long chatId = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            return $"chat_{chatId}";

        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = new string(name
                .Where(ch => !invalidChars.Contains(ch))
                .ToArray())
            .Replace(" ", "_")
            .Trim();

        if (string.IsNullOrWhiteSpace(safeName))
            return "Empty_Name_Chat";
        
        return safeName;
    }
}
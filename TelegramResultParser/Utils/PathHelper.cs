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
}
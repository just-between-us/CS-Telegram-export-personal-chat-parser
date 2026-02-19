using System.Text.Json;
using TelegramResultParser.Models;

namespace TelegramResultParser.Services;

public class NewTelegramParser
{
    public TelegramExportNewFormat ParseFile(string filePath)
    {
        Console.WriteLine($"Парсинг нового формата Telegram...");
        
        var jsonContent = File.ReadAllText(filePath);
        var export = JsonSerializer.Deserialize<TelegramExportNewFormat>(
            jsonContent, 
            new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            }
        ) ?? new TelegramExportNewFormat();
        
        foreach (var chat in export.GetAllChats())
        {
            Console.WriteLine($"Чат: {chat.Name}, Тип: {chat.Type}, Сообщений: {chat.Messages.Count}");
            var validMessages = new List<TelegramMessage>();
            foreach (var msg in chat.Messages)
            {
                if (msg != null && !string.IsNullOrWhiteSpace(msg.Text))
                {
                    // Очищаем текст
                    msg.Text = CleanText(msg.Text);
                    
                    validMessages.Add(msg);
                }
            }
            
            chat.Messages = validMessages.OrderBy(m => m.Date).ToList();
            
            Console.WriteLine($"  После очистки: {chat.Messages.Count} сообщений");
        }
        
        return export;
    }
    
    private string CleanText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";
        
        text = System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", string.Empty);
        
        text = text.Replace("\r\n", " ").Replace("\n", " ");
        
        while (text.Contains("  "))
            text = text.Replace("  ", " ");
            
        return text.Trim();
    }
    
    public List<TelegramChat> GetPersonalChats(TelegramExportNewFormat export)
    {
        return export.GetAllChats()
            .Where(chat => chat.IsPersonalChat && chat.Messages.Count > 10)
            .ToList();
    }
    
    public List<TelegramMessage> GetAllMessages(TelegramExportNewFormat export)
    {
        return export.GetAllChats()
            .SelectMany(chat => chat.Messages)
            .Where(msg => msg != null)
            .ToList();
    }
}
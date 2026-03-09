using System.Text.Json.Serialization;

namespace TelegramResultParser.Models
{
    public class ChatTelegramExport
    {
        [JsonPropertyName("name")]
        public string ChatName { get; set; } = string.Empty;
        
        [JsonPropertyName("type")]
        public string ChatType { get; set; } = string.Empty;
        
        [JsonPropertyName("id")]
        public long ChatId { get; set; }
        
        [JsonPropertyName("messages")]
        public List<TelegramMessage> Messages { get; set; } = new();
        
        [JsonIgnore]
        public int TotalMessages => Messages?.Count ?? 0;
        
        [JsonIgnore]
        public int MyMessages => Messages?.Count(m => m.IsFromMe) ?? 0;
        
        [JsonIgnore]
        public int OtherMessages => Messages?.Count(m => !m.IsFromMe) ?? 0;
        
        [JsonIgnore]
        public bool IsPersonalChat => ChatType?.Equals("personal_chat", StringComparison.OrdinalIgnoreCase) == true;
        
        public List<TelegramMessage> GetValidMessages() => 
            Messages?.Where(m => m.IsValid).ToList() ?? new List<TelegramMessage>();
    }
}
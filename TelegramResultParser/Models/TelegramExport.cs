using System.Text.Json.Serialization;

namespace TelegramResultParser.Models
{
    public class TelegramExportNewFormat
    {
        [JsonPropertyName("about")]
        public string About { get; set; } = string.Empty;
        
        [JsonPropertyName("chats")]
        public ChatList Chats { get; set; } = new();
        
        public List<TelegramChat> GetAllChats() => Chats?.List ?? new List<TelegramChat>();
    }
    
    public class ChatList
    {
        [JsonPropertyName("about")]
        public string About { get; set; } = string.Empty;
        
        [JsonPropertyName("list")]
        public List<TelegramChat> List { get; set; } = new();
    }
    
    public class TelegramChat
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("id")]
        public long Id { get; set; }
        
        [JsonPropertyName("messages")]
        public List<TelegramMessage> Messages { get; set; } = new();
        
        [JsonIgnore]
        public bool IsPersonalChat => Type == "personal_chat" || Type == "saved_messages";
        
        [JsonIgnore]
        public int MyMessagesCount => Messages?.Count(m => m.IsFromMe) ?? 0;
        
        [JsonIgnore]
        public int OtherMessagesCount => Messages?.Count(m => !m.IsFromMe) ?? 0;
        
        public List<TelegramMessage> GetValidMessages() => 
            Messages?.Where(m => m.IsValid).ToList() ?? new List<TelegramMessage>();
    }
}
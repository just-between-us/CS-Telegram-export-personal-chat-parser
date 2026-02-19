using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramResultParser.Models
{
    public class TelegramMessage
    {
        
        private const string MY_USER_ID = "";// USER_ID - измени на свой! (только цифры)
        private const string MY_NAME = ""; // можно добавить другие варианты имени (ник без собачки)
        
        [JsonPropertyName("id")]
        public long Id { get; set; }
        
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;
        
        [JsonPropertyName("from_id")]
        public string FromId { get; set; } = string.Empty;
        
        [JsonPropertyName("text")]
        [JsonConverter(typeof(NewTelegramTextConverter))]
        public string Text { get; set; } = string.Empty;
        
        [JsonPropertyName("text_entities")]
        public List<TextEntity> TextEntities { get; set; } = new();
        
        
        
        [JsonIgnore]
        public bool IsFromMe => FromId == MY_USER_ID || 
                            From?.Equals(MY_NAME, StringComparison.OrdinalIgnoreCase) == true;
        
        [JsonIgnore]
        public bool IsValid => !string.IsNullOrWhiteSpace(Text) && 
                            Text.Length >= 2 && 
                            !IsSystemMessage;
        
        [JsonIgnore]
        private bool IsSystemMessage => Text.Contains("присоединился") || 
                                    Text.Contains("покинул") || 
                                    Text.Contains("изменил") ||
                                    Text.Contains("удалил");
        [JsonIgnore]
        public string FullText
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Text))
                    return GetCleanText();
                
                if (TextEntities != null && TextEntities.Any())
                {
                    var text = string.Join(" ", TextEntities
                        .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Text))
                        .Select(e => e.Text));
                    
                    return CleanText(text);
                }
                
                return "";
            }
        }
        
        private string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            return text.Replace("\n", " ").Replace("\r", " ").Trim();
        }
        
        public string GetCleanText() 
        {
            if (string.IsNullOrWhiteSpace(Text)) return "";
            return Text.Replace("\n", " ").Trim();
        }
    }
    
    public class TextEntity
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
    
    public class NewTelegramTextConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return reader.GetString() ?? string.Empty;
            
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var parts = new List<string>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        parts.Add(reader.GetString() ?? "");
                    }
                    else if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        using var doc = JsonDocument.ParseValue(ref reader);
                        if (doc.RootElement.TryGetProperty("text", out var textProp))
                            parts.Add(textProp.GetString() ?? "");
                    }
                }
                return string.Join(" ", parts);
            }
            
            return string.Empty;
        }
        
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
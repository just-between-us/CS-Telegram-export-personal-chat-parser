namespace TelegramResultParser.Models
{
    public class TrainingExample
    {
        public string Input { get; set; } = string.Empty;    
        public string Output { get; set; } = string.Empty;   
        public string Context { get; set; } = string.Empty; 
        public DateTime Timestamp { get; set; }
        public string ChatName { get; set; } = string.Empty;
        public int InputLength => Input?.Length ?? 0;
        public int OutputLength => Output?.Length ?? 0;
        
        public bool IsValid => 
            !string.IsNullOrWhiteSpace(Input) && 
            !string.IsNullOrWhiteSpace(Output) &&
            Input.Length >= 3 && 
            Output.Length >= 3 &&
            !ContainsSensitiveInfo(Input) &&
            !ContainsSensitiveInfo(Output);
        
        private bool ContainsSensitiveInfo(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
                
            var lowerText = text.ToLowerInvariant();
            var sensitivePatterns = new[]
            {
                "парол", "password", "логин", "login",
                "карт", "card", "cvv", "cvc", "карточк",
                "паспорт", "сери", "номер паспорт",
                "+7", "+375", "+380", "номер тел", "адрес"
            };
            
            return sensitivePatterns.Any(pattern => lowerText.Contains(pattern));
        }
        
        public override string ToString() => 
            $"[{Timestamp:HH:mm}] {Input.Substring(0, Math.Min(30, Input.Length))}... → {Output.Substring(0, Math.Min(30, Output.Length))}...";
    }
}
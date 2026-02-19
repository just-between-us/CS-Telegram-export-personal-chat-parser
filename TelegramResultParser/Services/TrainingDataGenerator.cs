using Microsoft.Extensions.Logging;
using TelegramResultParser.Models;

namespace TelegramResultParser.Services
{
    public class TrainingDataGenerator(ILogger<TrainingDataGenerator>? logger = null)
    {
        public List<TrainingExample> GenerateExamples(TelegramExport export, int minMessages = 2)
        {
            var examples = new List<TrainingExample>();
            var validMessages = export.GetValidMessages();
            
            var messagesWithText = validMessages
                .Where(m => !string.IsNullOrWhiteSpace(m.FullText))
                .ToList();
            
            Console.WriteLine($"TDG: Сообщений с текстом: {messagesWithText.Count}");
            
            for (int i = 0; i < messagesWithText.Count - 1; i++)
            {
                var current = messagesWithText[i];
                var next = messagesWithText[i + 1];
                
                if (current.IsFromMe && !next.IsFromMe)
                {
                    var example = new TrainingExample
                    {
                        Input = current.FullText, 
                        Output = next.FullText,
                        Context = GetContext(messagesWithText, i, 5),
                        Timestamp = current.Date,
                        ChatName = export.ChatName
                    };
                    
                    if (example.IsValid)
                    {
                        examples.Add(example);
                    }
                }
            }
            
            return examples;
        }
        
        private string GetContext(List<TelegramMessage> messages, int currentIndex, int contextSize)
        {
            var contextMessages = new List<string>();
            int start = Math.Max(0, currentIndex - contextSize);
            
            for (int i = start; i <= currentIndex; i++)
            {
                var msg = messages[i];
                if (!msg.IsValid) continue;
                
                var prefix = msg.IsFromMe ? "Я" : msg.From;
                contextMessages.Add($"{prefix}: {msg.GetCleanText()}");
            }
            
            return string.Join("\n", contextMessages);
        }
        
        public List<TrainingExample> GenerateFromMultipleExports(List<TelegramExport> exports, int minExamplesPerChat = 10)
        {
            var allExamples = new List<TrainingExample>();
            
            logger?.LogInformation("Генерация примеров из {Count} экспортов", exports.Count);
            
            foreach (var export in exports)
            {
                var examples = GenerateExamples(export);
                if (examples.Count >= minExamplesPerChat)
                {
                    allExamples.AddRange(examples);
                    logger?.LogInformation("Чат '{ChatName}': добавлено {Examples} примеров", 
                        export.ChatName, examples.Count);
                }
                else
                {
                    logger?.LogWarning("Чат '{ChatName}': пропущен (мало примеров: {Examples})", 
                        export.ChatName, examples.Count);
                }
            }
            
            logger?.LogInformation("Итого сгенерировано {TotalExamples} примеров", allExamples.Count);

            return allExamples;
        }
        
        public Dictionary<string, int> GetStatistics(List<TrainingExample> examples)
        {
            return new Dictionary<string, int>
            {
                ["Всего примеров"] = examples.Count,
                ["Общая длина текста"] = examples.Sum(e => e.InputLength + e.OutputLength),
                ["Средняя длина запроса"] = examples.Any() ? (int)examples.Average(e => e.InputLength) : 0,
                ["Средняя длина ответа"] = examples.Any() ? (int)examples.Average(e => e.OutputLength) : 0,
                ["Уникальных чатов"] = examples.Select(e => e.ChatName).Distinct().Count()
            };
        }
    }
}
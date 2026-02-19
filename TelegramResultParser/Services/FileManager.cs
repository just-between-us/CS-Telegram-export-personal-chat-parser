using System.Text;
using System.Text.Json;
using CsvHelper;
using TelegramResultParser.Models;

namespace TelegramResultParser.Services
{
    public class FileManager
    {
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        public void SaveExamples(List<TrainingExample> examples, string basePath, bool saveAllFormats = true)
        {
            var directory = Path.GetDirectoryName(basePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var baseName = Path.GetFileNameWithoutExtension(basePath) ?? $"training_data_{timestamp}";
            var outputDir = Path.GetDirectoryName(basePath) ?? "output";
            
            var jsonPath = Path.Combine(outputDir, $"{baseName}.json");
            SaveAsJson(examples, jsonPath);
            
            var jsonlPath = Path.Combine(outputDir, $"{baseName}.jsonl");
            SaveAsJsonl(examples, jsonlPath);
            
            var csvPath = Path.Combine(outputDir, $"{baseName}.csv");
            SaveAsCsv(examples, csvPath);
            
            var txtPath = Path.Combine(outputDir, $"{baseName}.txt");
            SaveAsText(examples, txtPath);
            
            Console.WriteLine($"✅ Данные сохранены в:");
            Console.WriteLine($"   JSON:  {jsonPath}");
            Console.WriteLine($"   JSONL: {jsonlPath}");
            Console.WriteLine($"   CSV:   {csvPath}");
            Console.WriteLine($"   TXT:   {txtPath}");
        }
        
        private void SaveAsJson(List<TrainingExample> examples, string filePath)
        {
            var data = new
            {
                GeneratedAt = DateTime.Now,
                TotalExamples = examples.Count,
                Examples = examples
            };
            
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            File.WriteAllText(filePath, json);
        }
        
        private void SaveAsJsonl(List<TrainingExample> examples, string filePath)
        {
            var lines = new List<string>();
            
            foreach (var example in examples)
            {
                var openaiFormat = new
                {
                    messages = new object[]
                    {
                        new { role = "system", content = "Ты - пользователь Telegram. Отвечай кратко и естественно, как в личной переписке." },
                        new { role = "user", content = $"{example.Context}\n\n{example.Input}" },
                        new { role = "assistant", content = example.Output }
                    }
                };
                
                lines.Add(JsonSerializer.Serialize(openaiFormat));
            }
            
            File.WriteAllLines(filePath, lines);
        }
        
        private void SaveAsCsv(List<TrainingExample> examples, string filePath)
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);
            
            csv.WriteHeader<TrainingExample>();
            csv.NextRecord();
            
            foreach (var example in examples)
            {
                csv.WriteRecord(example);
                csv.NextRecord();
            }
        }
        
        private void SaveAsText(List<TrainingExample> examples, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== ТРЕНИРОВОЧНЫЕ ДАННЫЕ ===");
            sb.AppendLine($"Сгенерировано: {DateTime.Now}");
            sb.AppendLine($"Всего примеров: {examples.Count}");
            sb.AppendLine();
            
            for (int i = 0; i < Math.Min(50, examples.Count); i++)
            {
                var example = examples[i];
                sb.AppendLine($"Пример #{i + 1}");
                sb.AppendLine($"Чат: {example.ChatName}");
                sb.AppendLine($"Время: {example.Timestamp:g}");
                sb.AppendLine($"Контекст:\n{example.Context}");
                sb.AppendLine($"Вопрос: {example.Input}");
                sb.AppendLine($"Ответ: {example.Output}");
                sb.AppendLine(new string('-', 60));
                sb.AppendLine();
            }
            
            File.WriteAllText(filePath, sb.ToString());
        }
        
        public void SaveStatistics(Dictionary<string, int> stats, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== СТАТИСТИКА ===");
            sb.AppendLine($"Дата анализа: {DateTime.Now}");
            sb.AppendLine();
            
            foreach (var stat in stats)
            {
                sb.AppendLine($"{stat.Key}: {stat.Value}");
            }
            
            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
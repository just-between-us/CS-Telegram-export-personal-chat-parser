using Spectre.Console;
using TelegramResultParser.Models;
using TelegramResultParser.Services;
using TelegramResultParser.Utils;

namespace TelegramResultParser;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "Telegram Data Exporter";
        Console.WriteLine("=== Telegram Data Exporter ===");
        Console.WriteLine("");
        
        string[] jsonFiles = Directory.Exists("exports") 
            ? Directory.GetFiles("exports", "*.json", SearchOption.AllDirectories)
            : Array.Empty<string>();
        
        if (jsonFiles.Length == 0)
        {
            AnsiConsole.MarkupLine("[yellow]⚠️ Не найдено JSON файлов в папке 'exports'[/]");
            Console.WriteLine("Поместите экспорт Telegram в папку exports/");
            Console.WriteLine(@"Или ведите путь к JSON файлу экспорта, например: `C:\Users\admin\Desktop\экспорты\result.json` или `C:\Users\admin\Desktop\result.json`...");
            var filePath = Console.ReadLine()?.Trim('"') ?? throw new InvalidOperationException();
        
            if (string.IsNullOrEmpty(filePath)) 
            { 
                Console.WriteLine("Путь не указан.");
                return;
            }
            else
            {
                jsonFiles = PathHelper.ResolveJsonPath(filePath);
            }
        }
        
        Console.WriteLine($"Найдено файлов: {jsonFiles.Length}");
        
        var allExamples = new List<TrainingExample>();
        
        foreach (var file in jsonFiles)
        {
            Console.WriteLine();
            Console.WriteLine($"📁 Обработка: {Path.GetFileName(file)}");
            Console.WriteLine(new string('=', 50));
            
            try
            {
                var parser = new NewTelegramParser();
                var export = parser.ParseFile(file);
                
                var personalChats = parser.GetPersonalChats(export);
                Console.WriteLine($"📊 Личных чатов: {personalChats.Count}");
                
                if (personalChats.Count == 0)
                {
                    Console.WriteLine("⚠️ Не найдено личных диалогов");
                    continue;
                }
                
                var generator = new TrainingDataGenerator();
                var fileManager = new FileManager();
                
                foreach (var chat in personalChats)
                {
                    Console.WriteLine();
                    Console.WriteLine($"💬 Чат: {chat.Name},id: {chat.Id}");
                    Console.WriteLine($"   Сообщений: {chat.Messages.Count}");
                    Console.WriteLine($"   Моих: {chat.MyMessagesCount}");
                    Console.WriteLine($"   Других: {chat.OtherMessagesCount}");
                    
                    if (chat.Messages.Count < 20)
                    {
                        Console.WriteLine("   ⚠️ Слишком мало сообщений, пропускаем");
                        continue;
                    }
                    
                    var tempExport = new ChatTelegramExport
                    {
                        ChatName = chat.Name,
                        ChatType = chat.Type,
                        Messages = chat.Messages
                    };
                    
                    var examples = generator.GenerateExamples(tempExport);
                    Console.WriteLine($"🤖 Сгенерировано примеров: {examples.Count}");
                    
                    if (examples.Count > 0)
                    {
                        allExamples.AddRange(examples);
                        
                        string chatSafeName = PathHelper.MakeSafeFileName(chat.Name, chat.Id);
                        string outputPath = Path.Combine("output", "chats", 
                            $"{chatSafeName}_{DateTime.Now:yyyyMMdd_HHmm}");
                        
                        fileManager.SaveExamples(examples, outputPath);
                        
                        if (examples.Count > 0)
                        {
                            var sample = examples.First();
                            Console.WriteLine($"   Пример: \"{sample.Input.Substring(0, Math.Min(40, sample.Input.Length))}...\"");
                        }
                    }
                }
                
                Console.WriteLine();
                Console.WriteLine($"✅ Файл обработан. Всего примеров: {allExamples.Count}");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        
        if (allExamples.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"🎯 ИТОГО: {allExamples.Count} примеров из {jsonFiles.Length} файлов");
            
            var fileManager = new FileManager();
            string finalOutput = Path.Combine("output", $"ALL_TRAINING_{DateTime.Now:yyyyMMdd_HHmm}");
            fileManager.SaveExamples(allExamples, finalOutput);
            
            var generator = new TrainingDataGenerator();
            var stats = generator.GetStatistics(allExamples);
            
            Console.WriteLine();
            Console.WriteLine("📊 Статистика:");
            foreach (var stat in stats)
            {
                Console.WriteLine($"  {stat.Key}: {stat.Value}");
            }
            
            Console.WriteLine();
            AnsiConsole.MarkupLine($"[green]📁 Данные сохранены в: {Path.GetFullPath("output")}[/]");
            Console.WriteLine($"📄 Основной файл: {Path.GetFileName(finalOutput)}.jsonl");
            
            Console.WriteLine();
            Console.WriteLine($" Примеров: {allExamples.Count} (минимум нужно 500)");
            
            if (allExamples.Count < 500)
            {
                AnsiConsole.MarkupLine("[yellow]⚠️  Мало примеров! Добавьте больше экспортов Telegram.[/]");
            }
        }
        else
        {
            Console.WriteLine();
            AnsiConsole.MarkupLine("[red]❌ Не удалось сгенерировать ни одного примера.[/]");
            Console.WriteLine("Возможные причины:");
            Console.WriteLine("1. В экспорте нет личных диалогов");
            Console.WriteLine("2. Диалоги слишком короткие");
            Console.WriteLine("3. Формат экспорта неправильный");
        }
        
        Console.WriteLine();
        Console.WriteLine("Нажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}
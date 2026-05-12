using MockDataGenerator.Conventers;
using MockDataGenerator.Interfaces;
using MockDataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MockDataGenerator.Services
{
    public static class ComponentService
    {
        private static readonly string _componentFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Components");

        private static readonly JsonSerializerOptions options = new() 
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new ObjectToInferredTypesConverter(), new JsonStringEnumConverter() }
        };
        public static void InitComponentFolders()
        {
            Directory.CreateDirectory(_componentFolderPath);

            Directory.CreateDirectory(Path.Combine(_componentFolderPath, "ValueTypes"));

            Directory.CreateDirectory(Path.Combine(_componentFolderPath, "DBMSRules"));
        }

        public static void InitComponentSchemaTemplate(string Class)
        {
            string? schema = GetResourceText($"{Class}.schema.json");
            if (string.IsNullOrEmpty(schema))
                return;

            string folder = string.Empty;
            if (Class == "OutputValueType") folder = "ValueTypes";
            else if (Class == "DBMSRules") folder = Class;
            DirectoryInfo? directory = InitComponentFolder(folder);
            if (directory == null)
            {
                InitComponentFolders();
                directory = InitComponentFolder(folder);
            }
            File.WriteAllText(Path.Combine(directory!.FullName, $"{Class}.schema.json"), schema);

            if (!File.Exists(Path.Combine(directory!.FullName, $"{Class}_template.json")))
            {
                Dictionary<string, object> template;
                if (Class == "OutputValueType")
                {
                    template = new Dictionary<string, object>
                    {
                        ["$schema"] = $"./{Class}.schema.json",
                        ["DisplayName"] = "Отображаемое имя (число от 1 до 100)",
                        ["Type"] = "Integer",
                        ["GenerationType"] = "Number",
                        ["Data"] = new { Min = 1, Max = 100 }
                    };
                }
                else if (Class == "DBMSRules")
                {
                    template = new Dictionary<string, object>
                    {
                        ["$schema"] = $"./{Class}.schema.json",
                        ["DisplayName"] = "Отображаемое имя (MS SQL Server)",
                        ["StringChar"] = "'",
                        ["NameQuoteOpen"] = "[",
                        ["NameQuoteClose"] = "]",
                        ["EncodingCodePage"] = 65001,
                        ["DataFormats"] = new { 
                            String = "VARCHAR(100)",
                            Number = "INT",
                            Decimal = "DECIMAL",
                            Boolean = "BIT"
                        }
                    };
                }
                else return;

                var json = JsonSerializer.Serialize(template, options);
                File.WriteAllText(Path.Combine(directory!.FullName, $"{Class}_template.json"), json);
            }
        }

        private static string? GetResourceText(string resourceName)
        {
            if (resourceName == null)
                return null;
            resourceName = $"MockDataGenerator.{resourceName}";
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
                using StreamReader reader = new(stream);
                return reader.ReadToEnd();
            }
            catch(Exception)
            {
                return null;
            }
        }

        public static DirectoryInfo? InitComponentFolder(string folder)
        {
            string folderPath = Path.Combine(_componentFolderPath, folder);
            if (Directory.Exists(folderPath))
            {
                 return new DirectoryInfo(folderPath);
            } else { return null; }
        }

        public static void OpenComponentFolder()
        {
            InitComponentFolders();
            string path = _componentFolderPath;
            
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                else if (OperatingSystem.IsLinux())
                {
                    Process.Start(new ProcessStartInfo("xdg-open", $"\"{path}\"") { UseShellExecute = true });
                }
                else
                {
                    //Выдать сообщение о том, что ОС не поддеживается
                    return;
                }
            }
            catch(Exception)
            {
                //Выдать сообщение об ошибке
            }
        }

        private async static Task<T?> ReadFileAsync<T>(FileInfo file) where T : class
        {

            using FileStream fileStream = File.Open(file.FullName,
                                                    FileMode.Open,
                                                    FileAccess.Read,    
                                                    FileShare.ReadWrite);
            try
            {
                return await JsonSerializer.DeserializeAsync<T>(fileStream, options);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async static Task<IEnumerable<T>> GetComponentsAsync<T>()
            where T : class, IMockComponent
        {
            DirectoryInfo? componentsFolder = null;

            if (typeof(T) == typeof(OutputValueType))
                componentsFolder = InitComponentFolder("ValueTypes");
            else
                componentsFolder = InitComponentFolder("DBMSRules");

            if (componentsFolder == null) return [];

            var fileComponents = componentsFolder.GetFiles("*.json", SearchOption.TopDirectoryOnly);

            if (fileComponents.Length == 0) return [];
            //Читаем все файлы в папке
            var tasks = fileComponents.Select(file => ReadFileAsync<T>(file));
            //После чтения всех файлов записываем все, что прочитали.
            T?[] results = await Task.WhenAll(tasks);

            Dictionary<string, T> importComponents = new(fileComponents.Length,
                StringComparer.InvariantCultureIgnoreCase);

            foreach (var component in results)
            {
                if (component != null)
                {
                    importComponents.TryAdd(component.DisplayName.Trim(), component);
                }
            }

            return importComponents.Values;
        }
    }
}

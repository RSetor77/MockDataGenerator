using MockDataGenerator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MockDataGenerator.Services
{
    public static class ComponentService
    {
        private static readonly string _componentFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Components");
        public static void InitComponentFolders()
        {
            Directory.CreateDirectory(_componentFolderPath);

            Directory.CreateDirectory(Path.Combine(_componentFolderPath, "ValueTypes"));

            Directory.CreateDirectory(Path.Combine(_componentFolderPath, "DBMSRules"));
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
                return await JsonSerializer.DeserializeAsync<T>(fileStream);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async static Task<IEnumerable<OutputValueType>> GetComponentsAsync()
        {
            DirectoryInfo? componentsFolder = InitComponentFolder("ValueTypes");

            if (componentsFolder == null) return [];

            var fileComponents = componentsFolder.GetFiles("*.json", SearchOption.TopDirectoryOnly);

            if (fileComponents.Length == 0) return [];
            //Читаем все файлы в папке
            var tasks = fileComponents.Select(file => ReadFileAsync<OutputValueType>(file));
            //После чтения всех файлов записываем все, что прочитали.
            OutputValueType?[] results = await Task.WhenAll(tasks);

            Dictionary<string, OutputValueType> importComponents = new(fileComponents.Length, 
                StringComparer.InvariantCultureIgnoreCase);

            foreach(var component in results)
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

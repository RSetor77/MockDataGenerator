using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Jint;
using MockDataGenerator.Models;
using MockDataGenerator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Services
{
    
    public static class DialogService
    {
        public static async Task<T?> OpenModalWindow<T>() where T : class
        {
            var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows.FirstOrDefault(w => w.IsActive);
            if (owner == null)
                return default(T);
            var dialog = await IdentifyDialog<T>();

        }

        private static async Task<Window> IdentifyDialog<T>() where T : class
        {
            Window result = typeof(T) switch
            {
                var t when t == typeof(SelectWindowViewModel) => new SelectWindow(),
                var t when t == typeof(ComponentManagerViewModel) => new ComponentManager(),
                var t when t == typeof(OptionWindowViewModel) => new OptionWindow(),
                _ => throw new NotImplementedException(),
            };
            return result;
        }

        public static async Task<OutputValueType?> OpenSelectWindowAsync(OutputValueType[]? items)
        {
            var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows.FirstOrDefault(w => w.IsActive);
            if (owner == null)
                return null;
            var selectVM = new SelectWindowViewModel(items!);
            SelectWindow select = new(selectVM) { DataContext = selectVM };
            var result = await select.ShowDialog<OutputValueType>(owner);
            return result;
        }
        public static async Task<Components> OpenComponentManagerAsync()
        {
            var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows.FirstOrDefault(w => w.IsActive);
            if (owner == null)
                return new Components();
            ComponentManager manager = new();
            var result = await manager.ShowDialog<Components>(owner) ?? new Components();
            return result;

        }
        public static async Task<IStorageFile?> SaveFileAsync(GenerationOptions options)
        {
            var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows.FirstOrDefault(w => w.IsActive);
            if (owner == null)
                return null;
            TopLevel? topLevel = TopLevel.GetTopLevel(owner);

            if (topLevel == null)
                return null;

            string format = options.Format switch
            {
                FileFormats.TXT => "*.txt",
                FileFormats.CSV => "*.csv",
                FileFormats.SQL => "*.sql",
                FileFormats.JSON => "*.json",
                _ => "*.txt"
            };

            string formatTile = options.Format switch
            {
                FileFormats.TXT => "Текстовый файл",
                FileFormats.CSV => "Текстовый файл CSV",
                FileFormats.SQL => "Файл запроса SQL",
                FileFormats.JSON => "*Файл JSON",
                _ => "Текстовый файл"
            };

            string fileName = options.Format switch
            {
                FileFormats.SQL => (options.FormatSettings as SQLSettings)!.TableName!,
                _ => "Генерация_" + DateTime.Now.ToShortDateString()
            };

            var file = await topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Сохранить файл",
                FileTypeChoices = [new FilePickerFileType(formatTile) { Patterns = [format] }],
                DefaultExtension = format,
                SuggestedFileName = fileName
            });

            return file;
        }
    }
}

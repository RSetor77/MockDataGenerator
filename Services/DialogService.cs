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
        public static async Task<T?> OpenDialogWindow<T>(ViewModelBase vm) where T : class
        {
            var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows.FirstOrDefault(w => w.IsActive);
            if (owner == null)
                return default;
            var dialog = IdentifyDialog(vm);
            if (dialog == null) return default;
            return await dialog.ShowDialog<T>(owner);
        }

        private static Window? IdentifyDialog(ViewModelBase vm)
        {
            Window? result = vm switch
            {
                SelectWindowViewModel => new SelectWindow() { DataContext = vm },
                ComponentManagerViewModel => new ComponentManager() { DataContext = vm },
                OptionWindowViewModel => new OptionWindow() { DataContext = vm },
                _ => null,
            };
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

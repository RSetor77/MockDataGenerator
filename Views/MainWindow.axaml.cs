using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MockDataGenerator.Models;
using MockDataGenerator.ViewModels;
using System;
using System.Collections.Generic;

namespace MockDataGenerator.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel;
        public MainWindow()
        {
            InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                if (_viewModel != null)
                {
                    _viewModel.RequestComponents -= OnOpenComponentManager;
                }

                if (DataContext is MainWindowViewModel viewModel)
                {
                    _viewModel = viewModel;
                    _viewModel.RequestComponents += OnOpenComponentManager;
                    _viewModel.RequestSelect += OnRequestSelectWindow;
                    _viewModel.RequestFilePath += OnGenerate;
                }
            };
        }

        private async void OnOpenComponentManager(Action<Components> callback)
        {
            ComponentManager manager = new();
            var result = await manager.ShowDialog<Components>(this);
            if (result != null)
                callback(result);
            else callback(new Components());
        }

        //private async void OnOpenOptionWindow(Field? field, Action<Components> callback)
        //{
        //    ComponentManager manager = new();
        //    var result = await manager.ShowDialog<Components>(this);
        //    if (result != null)
        //        callback(result);
        //    else callback(new Components());
        //}

        private async void OnRequestSelectWindow(OutputValueType[]? items, Action<OutputValueType> callback)
        {
            var selectVM = new SelectWindowViewModel(items!);
            SelectWindow select = new(selectVM) { DataContext = selectVM };
            var result = await select.ShowDialog<OutputValueType>(this);
            if (result != null)
                callback(result);
            else callback(null!);
        }

        private async void SelectOutputValueType_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var btn = sender as Button;
            var FieldOutputType = btn!.CommandParameter as Field;

            var mainVm = (MainWindowViewModel)DataContext!;

            var dialogVm = new SelectWindowViewModel(mainVm.OutputValueTypes!);

            var dialog = new SelectWindow()
            {
                DataContext = dialogVm
            };

            await dialog.ShowDialog(this);

            if(dialogVm.SelectedValueType != null)
            {
                FieldOutputType!.OutputValueType = dialogVm.SelectedValueType;
            }
        }

        private async void OnGenerate(GenerationOptions options, Action<IStorageFile> callback)
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(this);

            if (topLevel == null)
                return;

            string format = options.Format switch
            {
                FileFormats.TXT => "*.txt",
                FileFormats.CSV => "*.csv",
                FileFormats.SQL => "*.sql",
                _ => "*.txt"
            };

            string formatTile = options.Format switch
            {
                FileFormats.TXT => "Текстовый файл",
                FileFormats.CSV => "Текстовый файл CSV",
                FileFormats.SQL => "Файл запроса SQL",
                _ => "Текстовый файл"
            };

            string fileName = options.Format switch
            {
                FileFormats.SQL => options.TableName!,
                _ => "Генерация_" + DateTime.Now.ToShortDateString()
            };

            var file = await topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Сохранить файл",
                FileTypeChoices = [ new FilePickerFileType(formatTile) { Patterns = [format] }],
                DefaultExtension = format,
                SuggestedFileName = fileName
            });

            callback(file!);
        }
    }
}
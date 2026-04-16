using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MockDataGenerator.Models;
using MockDataGenerator.Services;
using MockDataGenerator.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public event Action<Action<Components>>? RequestComponents;

        public event Action<OutputValueType[]?, Action<OutputValueType>>? RequestSelect;
        public event Action<GenerationOptions, Action<IStorageFile>>? RequestFilePath;
        public int RecordCount { get; set; } = 1;
        public string? TableName { get; set; }
        public bool CreateTable { get; set; } = false;
        public IEnumerable<FileFormats> Formats { get; } = Enum.GetValues<FileFormats>();
        public EncodingInfo[] Encodings { get; } = Encoding.GetEncodings();
        public ObservableCollection<Field> Fields { get; set; } = [];
        public OutputValueType[]? OutputValueTypes { get; set; }
        public DBMSRules[]? DBMSRules { get; set; }
        public bool CanAdd => Fields.Count < 1000;
        public string? TxtSeparator { get; set; }
        public bool CreateHeader { get; set; }

        [ObservableProperty]
        private string? _status = "Ожидание";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
        public DBMSRules? _selectedDBMSRules;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
        [NotifyPropertyChangedFor(nameof(IsSQLSelected))]
        [NotifyPropertyChangedFor(nameof(IsCSVSelected))]
        [NotifyPropertyChangedFor(nameof(IsTextSelected))]
        private FileFormats? _selectedFormat;
        public bool IsSQLSelected => SelectedFormat == FileFormats.SQL;
        public bool IsTextSelected => SelectedFormat == FileFormats.CSV || SelectedFormat == FileFormats.TXT;
        public bool IsCSVSelected => SelectedFormat == FileFormats.CSV;

        private bool CanGenerate
        {
            get {
                bool IsValid = false;
                bool IsTableValid = false;
                if (SelectedFormat == FileFormats.SQL)
                {
                    IsTableValid = Fields.Count > 0 && Fields.All(f => f.IsValid);
                    IsValid = SelectedDBMSRules != null && IsTableValid && string.IsNullOrEmpty(TableName);
                } 
                else if (SelectedFormat == FileFormats.TXT || SelectedFormat == FileFormats.CSV) 
                {
                    IsTableValid = Fields.Count > 0 && Fields.All(f => f.IsValid);
                    IsValid = SelectedEncoding != null && IsTableValid;
                }

                return IsValid;
            }
        }

        [ObservableProperty]
        public bool _useBOM;

        public bool IsUTFEncoding
        {
            get
            {
                if (SelectedEncoding == null) return false;

                int codepage = SelectedEncoding.CodePage;

                return codepage == 65001 || codepage == 1200 || codepage == 1201 || codepage == 12000;
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsUTFEncoding))]
        [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
        private EncodingInfo? _selectedEncoding;

        private void NotifyCommands()
        {
            AddFieldCommand.NotifyCanExecuteChanged();
            GenerateCommand.NotifyCanExecuteChanged();
        }

        private void OnFieldDataChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Field.IsValid))
            {
                NotifyCommands();
            }
        }

        public MainWindowViewModel()
        {
            Fields.CollectionChanged += (s, e) => 
            {
                if (e.NewItems != null)
                    foreach (Field item in e.NewItems)
                        item.PropertyChanged += OnFieldDataChanged;

                if (e.OldItems != null)
                    foreach (Field item in e.OldItems)
                        item.PropertyChanged -= OnFieldDataChanged;
                NotifyCommands();
            };

            
        }

        public void OpenComponentManager()
        {
            RequestComponents?.Invoke(result =>
            {
                OutputValueTypes = result.OutputValueTypes;
                DBMSRules = result.DBMSRules;
            });
        }

        [RelayCommand(CanExecute = nameof(CanAdd))]
        public void AddField()
        {
            Fields.Add(new());
        }

        [RelayCommand(CanExecute = nameof(CanAdd))]
        public void AddCopy()
        {
            if (Fields.Count >= 1)
            {
                byte last = Convert.ToByte(Fields.Count - 1);
                Fields.Add(Fields[last]);
            }
        }

        [RelayCommand]
        public void RemoveField(Field field)
        {
            Fields.Remove(field);
        }

        public void SelectOVT(Field field)
        {
            RequestSelect?.Invoke(OutputValueTypes, result =>
            {
                field.OutputValueType = result;
            });
        }

        [RelayCommand(CanExecute = nameof(CanGenerate))]
        public void Generate()
        {
            string? filePath = string.Empty;
            Encoding encoding;
            if (SelectedEncoding != null)
            {
                encoding = SelectedEncoding.CodePage switch
                {
                    65001 => new UTF8Encoding(UseBOM),
                    1200 => new UnicodeEncoding(false, UseBOM),
                    1201 => new UnicodeEncoding(true, UseBOM),
                    12000 => new UTF32Encoding(false, UseBOM),
                    12001 => new UTF32Encoding(true, UseBOM),
                    _ => Encoding.GetEncoding(SelectedEncoding.CodePage)
                };
            }
            else if (SelectedFormat == FileFormats.SQL) encoding = Encoding.GetEncoding(SelectedDBMSRules!.EncodingCodePage);
            else encoding = Encoding.UTF8;

            GenerationOptions options = new()
            {
                RecordsCount = Convert.ToUInt16(RecordCount),
                Format = SelectedFormat ?? FileFormats.TXT,
                Encoding = encoding,

                DBMS = (SelectedFormat == FileFormats.SQL) ? SelectedDBMSRules : null,
                TableName = (SelectedFormat == FileFormats.SQL) ? TableName : null,
                CreateTable = (SelectedFormat == FileFormats.SQL) && CreateTable,

                CreateHeader = (SelectedFormat == FileFormats.CSV) && CreateHeader,
                BOM = UseBOM,
                Separator = (SelectedFormat != FileFormats.SQL) ? TxtSeparator?[0] ?? ';' : ','
            };

            RequestFilePath?.Invoke(options, result =>
            {
                filePath = result.TryGetLocalPath();
                if (filePath != null)
                {
                    Console.WriteLine(filePath);
                }
            });
        }
    }
}

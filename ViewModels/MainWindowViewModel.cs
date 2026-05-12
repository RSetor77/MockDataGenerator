using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MockDataGenerator.Interfaces;
using MockDataGenerator.Models;
using MockDataGenerator.Services;
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
        public int RecordCount { get; set; } = 1;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
        private string? _tableName;
        public bool CreateTable { get; set; } = false;
        public IEnumerable<FileFormats> Formats { get; } = Enum.GetValues<FileFormats>();
        public EncodingInfo[] Encodings { get; } = [];
        public ObservableCollection<Field> Fields { get; set; } = [];
        public OutputValueType[]? OutputValueTypes { get; set; }
        public ObservableCollection<DBMSRules> DBMSRules { get; set; } = [];
        public bool CanAdd => Fields.Count < 1000;
        private bool CanMoveUp(Field field) 
        {
            if (field == null) return false;
            return Fields.IndexOf(field) > 0; 
        } 
        private bool CanMoveDown(Field field)
        { //index < Fields.Count - 1
            if (field == null) return false;
            return Fields.IndexOf(field) < Fields.Count - 1; 
        } 
        public string? TxtSeparator { get; set; }
        public bool CreateHeader { get; set; }
        public bool UseJsonLines { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
        public DBMSRules? _selectedDBMSRules;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
        [NotifyPropertyChangedFor(nameof(IsTextSelected))]
        [NotifyPropertyChangedFor(nameof(IsSQLSelected))]
        [NotifyPropertyChangedFor(nameof(IsCSVSelected))]
        [NotifyPropertyChangedFor(nameof(IsJsonSelected))]
        private FileFormats? _selectedFormat;

        partial void OnSelectedFormatChanged(FileFormats? value)
        {
            foreach(var field in Fields)
            {
                if ((field.IsPK || field.IsNotNull) && value != FileFormats.SQL)
                {
                    field.IsPK = false;
                    field.IsNotNull = false;
                }
            }
        }

        public bool IsTextSelected => SelectedFormat == FileFormats.TXT || SelectedFormat == FileFormats.CSV;
        public bool IsSQLSelected => SelectedFormat == FileFormats.SQL;
        public bool IsCSVSelected => SelectedFormat == FileFormats.CSV;
        public bool IsJsonSelected => SelectedFormat == FileFormats.JSON;

        private static IEnumerable<EncodingInfo> EncodingsSorted
        {
            get
            {
                var all = Encoding.GetEncodings();
                int[] priorityCodes = [65001, 1251, 1200, 866];
                var popular = all.Where(e => priorityCodes.Contains(e.CodePage)).OrderBy(e => Array.IndexOf(priorityCodes, e.CodePage));
                var others = all.Where(e => !priorityCodes.Contains(e.CodePage)).OrderBy(e => e.CodePage);
                var final = popular.Select(e => e).Concat(others.Select(e => e)).ToList();
                return [..final];
            }
        }

        private bool CanGenerate
        {
            get {
                bool IsValid = false;
                bool IsTableValid = false;
                if (SelectedFormat == FileFormats.SQL)
                {
                    IsTableValid = Fields.Count > 0 && Fields.All(f => f.IsValid);
                    IsValid = SelectedDBMSRules != null && IsTableValid && !string.IsNullOrEmpty(TableName);
                } 
                else if (SelectedFormat == FileFormats.TXT || SelectedFormat == FileFormats.CSV) 
                {
                    IsTableValid = Fields.Count > 0 && Fields.All(f => f.IsValid);
                    IsValid = SelectedEncoding != null && IsTableValid;
                } else
                {
                    IsValid = Fields.Count > 0 && Fields.All(f => f.IsValid) && SelectedFormat != null;
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

            Encodings = [..EncodingsSorted];
        }

        [RelayCommand(CanExecute = nameof(CanMoveUp))]
        public void MoveUp(Field field)
        {
            int currentIndex = Fields.IndexOf(field);
            if (currentIndex > 0)
            {
                // Метод Move встроен в ObservableCollection!
                Fields.Move(currentIndex, currentIndex - 1);
            }
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanMoveDown))]
        public void MoveDown(Field field)
        {
            int currentIndex = Fields.IndexOf(field);
            if (currentIndex < Fields.Count - 1)
            {
                Fields.Move(currentIndex, currentIndex + 1);
            }
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        public async Task SelectOVT(Field field)
        {
            if (OutputValueTypes == null)
                return;
            OutputValueType? result = await DialogService.OpenDialogWindow<OutputValueType>(new SelectWindowViewModel(OutputValueTypes));

            if (result == null)
                return;

            field.OutputValueType = result;
        }

        public async Task OpenComponentManager()
        {
            Components? result = await DialogService.OpenDialogWindow<Components>(new ComponentManagerViewModel());

            if (result == null)
                return;

            OutputValueTypes = result.OutputValueTypes;

            DBMSRules.Clear();
            foreach (DBMSRules rule in result.DBMSRules)
            {
                DBMSRules.Add(rule);
            }
        }

        [RelayCommand(CanExecute = nameof(CanAdd))]
        public void AddField()
        {
            Fields.Add(new());
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanAdd))]
        public void AddCopy()
        {
            if (Fields.Count >= 1)
            {
                byte last = Convert.ToByte(Fields.Count - 1);
                Fields.Add(Fields[last]);
            }
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        public void RemoveField(Field field)
        {
            Fields.Remove(field);
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanGenerate), AllowConcurrentExecutions = false)]
        public async Task Generate()
        {
            Encoding encoding = GetTargetEncoding();

            IFormatSettings? formatSettings = SelectedFormat switch
            {
                FileFormats.TXT => new TXTSettings()
                {
                    Encoding = encoding,
                    BOM = UseBOM,
                    Separator = string.IsNullOrEmpty(TxtSeparator) ? ';' : TxtSeparator[0]
                },

                FileFormats.CSV => new CSVSettings()
                {
                    Encoding = encoding,
                    BOM = UseBOM,
                    CreateHeader = CreateHeader,
                    Separator = string.IsNullOrEmpty(TxtSeparator) ? ';' : TxtSeparator[0]
                },
                FileFormats.SQL => new SQLSettings()
                {
                    DBMS = SelectedDBMSRules!,
                    TableName = TableName,
                    CreateTable = CreateTable,
                    Encoding = encoding
                },
                FileFormats.JSON => new JsonSettings() { JsonLines = UseJsonLines },
                _ => null
            };

            if (formatSettings == null)
                return;

            GenerationOptions options = new()
            {
                RecordsCount = Convert.ToUInt16(RecordCount),
                Format = SelectedFormat ?? FileFormats.TXT,
                FormatSettings = formatSettings
            };

            var result = await DialogService.SaveFileAsync(options);

            if (result == null)
                return;
            string? filePath = result.TryGetLocalPath();
            if (filePath != null)
            {
                await GenerationService.GenerateFile([.. Fields], result, options);
            }
        }

        private Encoding GetTargetEncoding()
        {
            if (SelectedEncoding != null)
            {
                return SelectedEncoding.CodePage switch
                {
                    65001 => new UTF8Encoding(UseBOM),
                    1200 => new UnicodeEncoding(false, UseBOM),
                    1201 => new UnicodeEncoding(true, UseBOM),
                    12000 => new UTF32Encoding(false, UseBOM),
                    12001 => new UTF32Encoding(true, UseBOM),
                    _ => Encoding.GetEncoding(SelectedEncoding.CodePage)
                };
            }

            if (SelectedFormat == FileFormats.SQL)
                return Encoding.GetEncoding(SelectedDBMSRules!.EncodingCodePage);

            return Encoding.UTF8;
        }
    }
}

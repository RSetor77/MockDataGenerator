using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MockDataGenerator.Models;
using MockDataGenerator.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace MockDataGenerator.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public event Action<Action<Components>>? RequestComponents;
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

        [ObservableProperty]
        private string? _status = "Ожидание";

        [ObservableProperty]
        public DBMSRules? _selectedDBMSRules;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsSQLSelected))]
        [NotifyPropertyChangedFor(nameof(IsTextSelected))]
        private FileFormats _selectedFormat;
        public bool IsSQLSelected => SelectedFormat == FileFormats.SQL;

        public bool IsTextSelected => SelectedFormat == FileFormats.CSV || SelectedFormat == FileFormats.TXT;

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
        private EncodingInfo? _selectedEncoding;

        private void NotifyCommands()
        {
            AddFieldCommand.NotifyCanExecuteChanged();
        }

        public MainWindowViewModel()
        {
            Fields.CollectionChanged += (s, e) => NotifyCommands();
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
    }
}

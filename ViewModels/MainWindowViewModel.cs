using CommunityToolkit.Mvvm.ComponentModel;
using MockDataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MockDataGenerator.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public int RecordCount { get; set; } = 1;
        public GenerationOptions GenerationOptions { get; set; } = new() { RecordsCount = 1 };
        public IEnumerable<FileFormats> Formats { get; } = Enum.GetValues<FileFormats>();
        public IEnumerable<FileEncodings> Encodings { get; } = Enum.GetValues<FileEncodings>();
        public ObservableCollection<Field>? Fields { get; set; }
        public DBMSRules[]? ListDBMS { get; set; }
        
        public MainWindowViewModel() {}

        public void OpenComponentManager()
        {
            ComponentManager ComponentManager = new() { DataContext = new ComponentManagerViewModel() };
            ComponentManager.Show();
        }
    }
}

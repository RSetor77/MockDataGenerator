using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public partial class Field : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsValid))]
        private string? _name = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsValid))]
        private OutputValueType? _outputValueType;

        public int Blank { get; set; } = 0;
        public Dictionary<string, int>? Options { get; set; }

        public bool IsValid 
        {
            get
            {
                return Name != null && OutputValueType != null;
            }
        }

        [RelayCommand]
        public void ClearOVT()
        {
            OutputValueType = null;
        }
    }
}

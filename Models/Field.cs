using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MockDataGenerator.Services;
using MockDataGenerator.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public bool? IsPK { get; set; }
        public bool? IsNotNull { get; set; }
        public string? Check { get; set; }

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

        

        [RelayCommand]
        public async Task UseOptions()
        {
            Field? result = await DialogService.OpenDialogWindow<Field>(new OptionWindowViewModel() { EditField = this });

            if (result == null)
                return;

            this.IsPK = result.IsPK;
            this.IsNotNull = result.IsNotNull;
            this.Check = result.Check;
        }
    }
}

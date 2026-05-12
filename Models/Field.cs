using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MockDataGenerator.Services;
using MockDataGenerator.ViewModels;
using System.Collections.Generic;
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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNullable))]
        private bool _isPK = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNullable))]
        private bool _isNotNull = false;
        public string Check { get; set; } = "";

        public bool IsValid 
        {
            get
            {
                return !string.IsNullOrEmpty(Name) && OutputValueType != null;
            }
        }

        public bool IsNullable
        {
            get
            {
                return !IsPK && !IsNotNull;
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

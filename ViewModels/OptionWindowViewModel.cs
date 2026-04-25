using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using MockDataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.ViewModels
{
    public partial class OptionWindowViewModel: ViewModelBase
    {
        public Field EditField { get; set; } = new();

        public OptionWindowViewModel() { }

        [RelayCommand]
        public void SelectAndClose(Window window)
        {
            window.Close(EditField);
        }
    }
}

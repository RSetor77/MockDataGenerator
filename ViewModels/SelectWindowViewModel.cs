using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using MockDataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.ViewModels
{
    public partial class SelectWindowViewModel: ViewModelBase
    {
        public ObservableCollection<OutputValueType> ValueTypes { get; } = [];

        public OutputValueType? SelectedValueType { get; set; }

        public SelectWindowViewModel() { }

        public SelectWindowViewModel(List<OutputValueType> values)
        {
            ValueTypes = new(values);
        }

        public SelectWindowViewModel(OutputValueType[]? values)
        {
            if (values != null)
                ValueTypes = new(values);
        }

        [RelayCommand]
        public void SelectAndClose(Window window)
        {
            window.Close(SelectedValueType);
        }
    }
}

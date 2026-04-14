using MockDataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.ViewModels
{
    public class SelectWindowViewModel: ViewModelBase
    {
        public ObservableCollection<OutputValueType> ValueTypes { get; } = new();

        public OutputValueType? SelectedValueType { get; set; }

        public SelectWindowViewModel() { }

        public SelectWindowViewModel(List<OutputValueType> values)
        {
            ValueTypes = new(values);
        }
    }
}

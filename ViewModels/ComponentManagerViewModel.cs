using MockDataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.ViewModels
{
    public partial class ComponentManagerViewModel : ViewModelBase
    {
        public ObservableCollection<OutputValueTypeHeader> OutputValueTypeHeaders { get; set; }

        public IList<OutputValueTypeHeader> SelectedOutputValueTypes { get; set; } = [];

        public ComponentManagerViewModel()
        {
            OutputValueTypeHeaders = [ 
                new() { DisplayName = "Тест1", Type = ValueTypes.Decimal},
                new() { DisplayName = "Тест2", Type = ValueTypes.Integer},
                new() { DisplayName = "Тест3", Type = ValueTypes.String},
                new() { DisplayName = "Тест4", Type = ValueTypes.Boolean}
            ];
        }

        public void Test()
        {

        }
    }
}

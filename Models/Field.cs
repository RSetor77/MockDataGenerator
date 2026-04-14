using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public partial class Field: ObservableObject
    {
        public string? Name { get; set; }

        [ObservableProperty]
        private OutputValueType? _outputValueType;

        public int Blank { get; set; } = 0;
        public Dictionary<string, int>? Options { get; set; }
    }
}

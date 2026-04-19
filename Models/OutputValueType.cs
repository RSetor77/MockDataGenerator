using MockDataGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public class OutputValueType: IMockComponent
    {
        public required string DisplayName { get; set; }
        public required ValueTypes Type { get; set; }
        public required GenerationTypes GenerationType { get; set; }
        public required Dictionary<string, object> Data { get; set; }
        public string? CustomTypeKey { get; set; }
    }
}

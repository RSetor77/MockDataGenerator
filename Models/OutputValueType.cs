using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public class OutputValueType
    {
        public required string DisplayName { get; set; }
        public ValueTypes Type { get; set; }
        public GenerationTypes GenerationType { get; set; }
        public required object[] Data { get; set; }
    }
}

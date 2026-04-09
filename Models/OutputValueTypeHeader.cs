using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public class OutputValueTypeHeader
    {
        public required string DisplayName { get; set; }
        public ValueTypes Type { get; set; }
        public GenerationTypes GenerationType { get; set; }
    }
}

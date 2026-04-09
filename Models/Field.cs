using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public class Field
    {
        public string? Name { get; set; }
        public OutputValueType? OutputValueType { get; set; }
        public int Blank { get; set; } = 0;
        public Dictionary<string, int>? Options { get; set; }
    }
}

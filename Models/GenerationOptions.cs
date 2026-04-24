using MockDataGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public class GenerationOptions
    {
        public required ushort RecordsCount { get; set; } = 1;
        public required FileFormats Format { get; set; }
        public required IFormatSettings FormatSettings { get; set; }
    }
}

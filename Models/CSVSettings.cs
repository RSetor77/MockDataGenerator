using MockDataGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public class CSVSettings : IFormatSettings
    {
        public bool CreateHeader { get; set; } = false;
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public bool BOM { get; set; } = false;
        public char Separator { get; set; } = ',';
    }
}

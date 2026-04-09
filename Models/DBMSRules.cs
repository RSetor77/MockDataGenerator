using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public class DBMSRules
    {
        public required string DisplayName { get; set; }
        public char StringChar { get; set; } = '"';
        public char NameQuoteChar { get; set; } = '\'';
        public required Dictionary<string, string> DataFormats { get; set; }
    }
}

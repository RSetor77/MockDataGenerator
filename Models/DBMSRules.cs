using Avalonia.Input;
using MockDataGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public class DBMSRules : IMockComponent
    {
        public required string DisplayName { get; set; }
        public char StringChar { get; set; } = '\'';
        public char NameQuoteChar { get; set; } = '\'';
        public int EncodingCodePage { get; set; } = 65001;
        public bool CompactInsert { get; set; }
        public required Dictionary<string, string> DataFormats { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public static readonly Dictionary<string, string> SystemDefaults = new(StringComparer.OrdinalIgnoreCase)
        {
            { "String", "VARCHAR(MAX)" },
            { "Number", "INT" },
            { "Decimal", "DECIMAL" },
            { "Boolean", "TINYINT" }
        };
        
    }
}

using MockDataGenerator.Interfaces;
using System;
using System.Collections.Generic;

namespace MockDataGenerator.Models
{
    public class DBMSRules : IMockComponent
    {
        public required string DisplayName { get; set; }
        public char StringChar { get; set; } = '\'';
        public char NameQuoteOpen { get; set; } = '\'';

        private char? _nameQuoteClose;

        public char NameQuoteClose
        {
            get => _nameQuoteClose ?? NameQuoteOpen;
            set => _nameQuoteClose = value;
        }

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

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
        public DBMSRules? DBMS {  get; set; }
        public string? TableName { get; set; }
        public bool CreateTable { get; set; }
        public bool CreateHeader { get; set; }
        public required Encoding Encoding { get; set; }
        public bool BOM { get; set; }
        public char Separator { get; set; }
    }
}

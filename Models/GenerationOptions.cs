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
        public FileFormats Format { get; set; }
        public DBMSRules? DBMS {  get; set; }
        public string? TableName { get; set; }
        public bool NeedCreateTable { get; set; }
        public FileEncodings Encoding { get; set; }
        public char? Separator { get; set; }
    }
}

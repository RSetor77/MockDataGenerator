using MockDataGenerator.Interfaces;
using System.Text;

namespace MockDataGenerator.Models
{
    public class SQLSettings: IFormatSettings
    {
        public DBMSRules DBMS { get; set; } = new() { DisplayName = "Empty", DataFormats = [] };
        public string? TableName { get; set; }
        public bool CreateTable { get; set; } = false;
        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }
}

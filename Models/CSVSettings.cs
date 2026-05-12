using MockDataGenerator.Interfaces;
using System.Text;

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

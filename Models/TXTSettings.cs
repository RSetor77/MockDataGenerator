using MockDataGenerator.Interfaces;
using System.Text;

namespace MockDataGenerator.Models
{
    public class TXTSettings: IFormatSettings
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public bool BOM { get; set; } = false;
        public char Separator { get; set; } = ',';
    }
}

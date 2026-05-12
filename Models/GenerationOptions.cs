using MockDataGenerator.Interfaces;

namespace MockDataGenerator.Models
{
    public class GenerationOptions
    {
        public required ushort RecordsCount { get; set; } = 1;
        public required FileFormats Format { get; set; }
        public required IFormatSettings FormatSettings { get; set; }
    }
}

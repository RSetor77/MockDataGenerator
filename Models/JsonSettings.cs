using MockDataGenerator.Interfaces;

namespace MockDataGenerator.Models
{
    public class JsonSettings : IFormatSettings
    {
        public bool JsonLines { get; set; } = false;
    }
}

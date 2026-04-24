using MockDataGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public class JsonSettings : IFormatSettings
    {
        public bool JsonLines { get; set; } = false;
    }
}

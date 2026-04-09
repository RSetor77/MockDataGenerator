using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    

    public class FileFormat
    {
        public required string Name { get; set; }
        public FileFormats Format { get; set; }
    }
}

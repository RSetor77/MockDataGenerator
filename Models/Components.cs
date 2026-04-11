using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public class Components
    {
        public OutputValueType[] OutputValueTypes { get; set; } = [];

        public DBMSRules[] DBMSRules { get; set; } = [];
    }
}

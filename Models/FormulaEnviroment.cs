using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public class FormulaEnviroment
    {
        public int Index { get; set; }
        public int? Iteration { get; set; }
        public Dictionary<string, object> Rows { get; set; } = [];
    }
}

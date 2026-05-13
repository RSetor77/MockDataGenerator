using System.Collections.Generic;

namespace MockDataGenerator.Models
{
    public struct FormulaEnvironment
    {
        public int Index { get; set; }
        public int? Iteration { get; set; }
        public Dictionary<string, object?> Rows { get; set; }
    }
}

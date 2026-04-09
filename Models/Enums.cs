using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public enum FileFormats
    {
        TXT,
        CSV,
        SQL
    }

    public enum ValueTypes
    {
        String,
        Integer,
        Decimal,
        Boolean
    }

    public enum GenerationTypes
    {
        Formula,
        Number,
        Array,
        Boolean,
        Increment
    }

    public enum FileEncodings
    {
        UTF8,
        UTF8BOM,
        ANSI
    }
}

namespace MockDataGenerator.Models
{
    public enum FileFormats
    {
        TXT,
        CSV,
        SQL,
        JSON
    }

    public enum ValueTypes
    {
        String,
        Integer,
        Decimal,
        Boolean,
        Custom,
        QuotedCustom
    }

    public enum GenerationTypes
    {
        None,
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

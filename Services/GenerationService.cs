using Avalonia.Input;
using Avalonia.Platform.Storage;
using Jint;
using MockDataGenerator.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.Services
{
    public static class GenerationService
    {
        public static async Task GenerateFile(Field[] fields, IStorageFile file, GenerationOptions options)
        {
            await using var stream = await file.OpenWriteAsync();


            switch (options.Format)
            {
                case FileFormats.TXT:
                    await GenerateTXT(stream, fields, options);
                    break;
                case FileFormats.CSV:
                    await GenerateCSV(stream, fields, options);
                    break;
                case FileFormats.SQL:
                    await GenerateSQL(stream, fields, options);
                    break;

            }
        }

        public static async Task GenerateTXT(Stream stream, Field[] fields, GenerationOptions options)
        {
            await using var writer = new StreamWriter(stream, options.Encoding);

            for (ushort i = 0; i < options.RecordsCount; i++)
            {
                //Генерируем данные
                for (ushort j = 0; j < fields.Length; j++)
                {
                    await writer.WriteAsync(GenerateData(fields[j].OutputValueType!, fields[j].Blank, i, null!));
                    if (j + 1 < fields.Length)
                        await writer.WriteAsync(options.Separator);
                }
                await writer.WriteLineAsync(string.Empty);
            }
        }

        public static async Task GenerateCSV(Stream stream, Field[] fields, GenerationOptions options)
        {
            await using var writer = new StreamWriter(stream, options.Encoding);

            if (options.CreateHeader) await WriteHeader(writer, fields, options.Separator);

            for (ushort i = 0; i < options.RecordsCount; i++)
            {
                //Генерируем данные
                for (ushort j = 0; j < fields.Length; j++)
                {
                    string rawData = GenerateData(fields[j].OutputValueType!, fields[j].Blank, i, null!);

                    if (!rawData.Equals("Null", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (fields[j].OutputValueType!.Type == ValueTypes.String)
                        {
                            string formattedData = $"\"{rawData.Replace("\"", "\"\"")}\"";

                            await writer.WriteAsync(formattedData);
                        }
                        else await writer.WriteAsync(rawData);
                        
                    }

                    if (j + 1 < fields.Length)
                        await writer.WriteAsync(options.Separator);
                }
                await writer.WriteLineAsync(string.Empty);
            }
        }

        public static async Task GenerateSQL(Stream stream, Field[] fields, GenerationOptions options)
        {
            await using var writer = new StreamWriter(stream, options.Encoding);

            if (options.CreateTable) await AddCreateTableQuery(writer, fields, options.DBMS!, options.TableName!);

            if (options.DBMS!.CompactInsert) await AddInsertValuesQueryHeader(writer, fields, options.DBMS!, options.TableName!);

            for (ushort i = 0; i < options.RecordsCount; i++)
            {
                if (!options.DBMS!.CompactInsert) await AddInsertValuesQueryStart(writer, fields, options.DBMS!, options.TableName!);
                else await writer.WriteAsync("(");

                //Генерируем данные
                for (ushort j = 0; j < fields.Length; j++)
                {
                    string rawData = GenerateData(fields[j].OutputValueType!, fields[j].Blank, i, options.DBMS!);

                    if (fields[j].OutputValueType!.Type == ValueTypes.String && !rawData.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                    {
                        char quote = options.DBMS!.StringChar;
                        string formattedData = $"{quote}{rawData.Replace(quote.ToString(), new string(quote, 2))}{quote}";

                        await writer.WriteAsync(formattedData);
                    }
                    else await writer.WriteAsync(rawData);


                    if (j + 1 < fields.Length) await writer.WriteAsync(',');
                }

                if (i < options.RecordsCount - 1) await writer.WriteLineAsync(options.DBMS!.CompactInsert ? ")," : ");");
                else await writer.WriteLineAsync(");");
            }
        }

        private static async Task WriteHeader(StreamWriter writer, Field[] fields, char separator)
        {
            for(ushort i=0; i < fields.Length; i++)
            {
                string fieldName = fields[i].Name!;
                if ((i + 1) < fields.Length)
                    fieldName += separator;
                await writer.WriteAsync(fieldName);
            }
            await writer.WriteLineAsync();
        }

        private static async Task AddCreateTableQuery(StreamWriter writer, Field[] fields, DBMSRules rules, string TableName)
        {
            await writer.WriteLineAsync($"CREATE TABLE {TableName} (");
            for(ushort i=0; i < fields.Length; i++)
            {
                string formattedType = fields[i].OutputValueType!.Type switch
                {
                    ValueTypes.Integer => rules.DataFormats.GetValueOrDefault("Number", DBMSRules.SystemDefaults["Number"]),
                    ValueTypes.String => rules.DataFormats.GetValueOrDefault("String", DBMSRules.SystemDefaults["String"]),
                    ValueTypes.Decimal => rules.DataFormats.GetValueOrDefault("Decimal", DBMSRules.SystemDefaults["Decimal"]),
                    ValueTypes.Boolean => rules.DataFormats.GetValueOrDefault("Boolean", DBMSRules.SystemDefaults["Boolean"]),
                    ValueTypes.Custom => rules.DataFormats.GetValueOrDefault(fields[i].OutputValueType!.CustomTypeKey ?? "String", DBMSRules.SystemDefaults["String"]),
                    _ => throw new ArgumentOutOfRangeException(nameof(fields), "Неизвестный тип данных")
                };
                string fieldString = $"\t{rules.NameQuoteOpen}{fields[i].Name}{rules.NameQuoteClose} {formattedType}";

                //Ограничения

                if ((i + 1) < fields.Length)
                    fieldString += ',';
                
                await writer.WriteLineAsync(fieldString);
            }
            await writer.WriteLineAsync(");");
            await writer.WriteLineAsync();
        }

        private static async Task AddInsertValuesQueryHeader(StreamWriter writer, Field[] fields, DBMSRules rules, string TableName)
        {
            await writer.WriteAsync($"INSERT INTO {TableName}(");
            for (ushort i = 0; i < fields.Length; i++)
            {
                string fieldName = $"{rules.NameQuoteOpen}{fields[i].Name}{rules.NameQuoteClose}";
                if (i + 1 < fields.Length)
                    fieldName += ",";
                await writer.WriteAsync(fieldName);
            }
            await writer.WriteLineAsync($") VALUES");
        }

        private static async Task AddInsertValuesQueryStart(StreamWriter writer, Field[] fields, DBMSRules rules, string TableName)
        {
            await writer.WriteAsync($"INSERT INTO {TableName}(");
            for(ushort i = 0; i < fields.Length; i++)
            {
                string fieldName = $"{rules.NameQuoteOpen}{fields[i].Name}{rules.NameQuoteClose}";
                if (i + 1 < fields.Length)
                    fieldName += ",";
                await writer.WriteAsync(fieldName);
            }
            await writer.WriteAsync($") VALUES (");
        }

        private static string GenerateData(OutputValueType type, int BlankChance, ushort counter, DBMSRules rules)
        {
            string value;
            if (BlankChance > 0)
            {
                bool isBlank = Random.Shared.NextSingle() < (Convert.ToSingle(BlankChance) / 100);
                if (isBlank)
                {
                    value = "NULL";
                    return value;
                }
            }
            try
            {
                switch (type.GenerationType)
                {
                    case GenerationTypes.Number:
                        if (type.Type == ValueTypes.Integer)
                        {
                            int min = type.Data.TryGetValue("Min", out object? resultMin) ? Convert.ToInt32(resultMin) : 0;
                            int max = type.Data.TryGetValue("Max", out object? resultMax) ? Convert.ToInt32(resultMax) : 99;
                            int number = Random.Shared.Next(min, max);
                            value = number.ToString();
                        } else if (type.Type == ValueTypes.Decimal)
                        {
                            double min = type.Data.TryGetValue("Min", out object? resultMin) ? Convert.ToDouble(resultMin) : 0;
                            double max = type.Data.TryGetValue("Max", out object? resultMax) ? Convert.ToDouble(resultMax) : 99;
                            decimal number = Convert.ToDecimal(Random.Shared.NextDouble() * (max - min) + min);
                            value = number.ToString(CultureInfo.InvariantCulture);
                        } else throw new Exception("Invalid Parameters");
                        break;
                    case GenerationTypes.Array:
                        var arrayData = (List<object>)(type.Data.GetValueOrDefault("Array"))! ?? throw new Exception("Invalid Array");
                        if (type.Type != ValueTypes.String) type.Type = ValueTypes.String;
                        value = arrayData[Random.Shared.Next(arrayData.Count)].ToString()!;
                        break;
                    case GenerationTypes.Increment:
                        int StartVal = Convert.ToInt32(type.Data.GetValueOrDefault("Start"));
                        int Step = Convert.ToInt32(type.Data.GetValueOrDefault("Step"));
                        value = (StartVal + (counter * Step)).ToString();
                        break;
                    case GenerationTypes.Boolean:
                        bool data = Random.Shared.Next(2) == 0;
                        value = data.ToString();
                        break;
                    case GenerationTypes.Formula:
                        //Добавить проверку на ValueTypes 
                        //Парсинг строки кода JS из Data["Formula"]
                        value = type.GetFormulaOutput();
                        break;
                    default: throw new Exception("Invalid Generation Type");
                }
            }
            catch(Exception ex) 
            {
                //Вывести ошибку в поле. Пока что WriteLine, но это будет изменено.
                Console.WriteLine(ex.Message);
                value = "'invalid'";
                type.Type = ValueTypes.String;
            }
                
            return value;
        }
    }
}

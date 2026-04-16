using Avalonia.Platform.Storage;
using MockDataGenerator.Models;
using System;
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
            await using var stream = await file.OpenReadAsync();
            await using var writer = new StreamWriter(stream, options.Encoding);
            switch (options.Format)
            {
                case FileFormats.CSV:
                    if (options.CreateHeader) await WriteHeader(writer, fields, options.Separator);
                    break;
                case FileFormats.SQL:
                    if (options.CreateTable) await AddCreateTableQuery(writer, fields, options.DBMS!, options.TableName!);
                    break;

            }
            if (options.Format == FileFormats.SQL && options.DBMS!.CompactInsert)
                await AddInsertValuesQueryHeader(writer, fields, options.DBMS!, options.TableName!);
            
            
            for(ushort i=0;i<options.RecordsCount;i++)
            {
                if (options.Format == FileFormats.SQL)
                {
                    if(!options.DBMS!.CompactInsert)
                        await AddInsertValuesQueryStart(writer, fields, options.DBMS!, options.TableName!);
                    else await writer.WriteAsync("(");
                }

                //Генерируем данные
                for(ushort j=0;j<fields.Length;j++)
                {
                    //await GenerateData(writer);
                    if (j + 1 < fields.Length)
                        await writer.WriteAsync(options.Format == FileFormats.SQL ? ',' : options.Separator);
                }
                if (options.Format == FileFormats.SQL)
                {
                    if (i < options.RecordsCount - 1)
                        await writer.WriteLineAsync(options.DBMS!.CompactInsert ? ")," : ");");
                    else await writer.WriteLineAsync(");");
                } else await writer.WriteLineAsync(string.Empty);
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
                    ValueTypes.Integer => rules.DataFormats["Number"],
                    ValueTypes.String => rules.DataFormats["String"],
                    ValueTypes.Decimal => rules.DataFormats["Decimal"],
                    ValueTypes.Boolean => rules.DataFormats["Boolean"],
                    _ => throw new ArgumentOutOfRangeException(nameof(fields), "Неизвестный тип данных")
                };
                string fieldString = $"\t{rules.NameQuoteChar}{fields[i].Name}{rules.NameQuoteChar} {formattedType}";

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
                string fieldName = $"{rules.NameQuoteChar}{fields[i].Name}{rules.NameQuoteChar}";
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
                string fieldName = $"{rules.NameQuoteChar}{fields[i].Name}{rules.NameQuoteChar}";
                if (i + 1 < fields.Length)
                    fieldName += ",";
                await writer.WriteAsync(fieldName);
            }
            await writer.WriteAsync($") VALUES (");
        }

        private static string GenerateData(OutputValueType type, int BlankChance, ushort counter, DBMSRules rules)
        {
            string value = string.Empty;
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
                        string[] arrayData = (string[]?)(type.Data.GetValueOrDefault("Array")) ?? throw new Exception("Invalid Array");
                        value = arrayData[Random.Shared.Next(arrayData.Length)];
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
                        //Парсинг строки кода C# из Data["Formula"]
                        break;
                    default: throw new Exception("Invalid Generation Type");
                }
            }
            catch(Exception ex) 
            {
                //Вывести ошибку в поле. Пока что WriteLine, но это будет изменено.
                Console.WriteLine(ex.Message);
                value = "invalid";
            }
            //rules != null это значит генерируемый файл - SQL.
            if (rules != null && type.Type == ValueTypes.String)
            {
                value = value.Replace("'", "''");
                value = $"'{value}'";
            }
                
            return value;
        }
    }
}

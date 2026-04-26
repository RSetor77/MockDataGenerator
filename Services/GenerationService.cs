using Avalonia.Input;
using Avalonia.Platform.Storage;
using MockDataGenerator.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MockDataGenerator.Services
{
    public static class GenerationService
    {
        private static readonly JsonSerializerOptions jsonOptions = new() { 
            WriteIndented = true, 
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static readonly JsonSerializerOptions jsonLinesOptions = new()
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static async Task GenerateFile(Field[] fields, IStorageFile file, GenerationOptions options)
        {
            await using var stream = await file.OpenWriteAsync();


            switch (options.FormatSettings)
            {
                case TXTSettings:
                    await GenerateTXT(stream, fields, options);
                    break;
                case CSVSettings:
                    await GenerateCSV(stream, fields, options);
                    break;
                case SQLSettings:
                    await GenerateSQL(stream, fields, options);
                    break;
                case JsonSettings:
                    await GenerateJSON(stream, fields, options);
                    break;
                default:
                    Debug.WriteLine("Попытка сгенерировать файл с неизвестными настройками.");
                    break;

            }
        }

        public static async Task GenerateTXT(Stream stream, Field[] fields, GenerationOptions options)
        {
            TXTSettings settings = (TXTSettings)options.FormatSettings;
            await using var writer = new StreamWriter(stream, settings.Encoding);

            for (ushort i = 0; i < options.RecordsCount; i++)
            {
                //Генерируем данные
                for (ushort j = 0; j < fields.Length; j++)
                {
                    string rawData = GenerateDataString(fields[j].OutputValueType!, fields[j].Blank, i);
                    if (rawData == "Undefined") continue;
                    await writer.WriteAsync(rawData);
                    if (j + 1 < fields.Length)
                        await writer.WriteAsync(settings.Separator);
                }
                await writer.WriteLineAsync(string.Empty);
            }
        }

        public static async Task GenerateCSV(Stream stream, Field[] fields, GenerationOptions options)
        {
            CSVSettings settings = (CSVSettings)options.FormatSettings;
            await using var writer = new StreamWriter(stream, settings.Encoding);

            if (settings.CreateHeader) await WriteHeader(writer, fields, settings.Separator);

            for (ushort i = 0; i < options.RecordsCount; i++)
            {
                //Генерируем данные
                for (ushort j = 0; j < fields.Length; j++)
                {
                    string rawData = GenerateDataString(fields[j].OutputValueType!, fields[j].Blank, i);

                    if (rawData == "Undefined") continue;

                    if (!rawData.Equals("Null", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (fields[j].OutputValueType!.Type == ValueTypes.String || fields[j].OutputValueType!.Type == ValueTypes.QuotedCustom)
                        {
                            string formattedData = $"\"{rawData.Replace("\"", "\"\"")}\"";

                            await writer.WriteAsync(formattedData);
                        }
                        else await writer.WriteAsync(rawData);
                        
                    }

                    if (j + 1 < fields.Length)
                        await writer.WriteAsync(settings.Separator);
                }
                await writer.WriteLineAsync(string.Empty);
            }
        }

        public static async Task GenerateSQL(Stream stream, Field[] fields, GenerationOptions options)
        {
            SQLSettings settings = (SQLSettings)options.FormatSettings;
            await using var writer = new StreamWriter(stream, settings.Encoding);

            if (settings.CreateTable) await AddCreateTableQuery(writer, fields, settings.DBMS!, settings.TableName!);

            if (settings.DBMS!.CompactInsert) await AddInsertValuesQueryHeader(writer, fields, settings.DBMS!, settings.TableName!);

            for (ushort i = 0; i < options.RecordsCount; i++)
            {
                if (!settings.DBMS!.CompactInsert) await AddInsertValuesQueryStart(writer, fields, settings.DBMS!, settings.TableName!);
                else await writer.WriteAsync("(");

                //Генерируем данные
                for (ushort j = 0; j < fields.Length; j++)
                {
                    string rawData = GenerateDataString(fields[j].OutputValueType!, fields[j].IsNullable ? fields[j].Blank : 0, i);

                    if (rawData == "Undefined") continue;

                    if ((fields[j].OutputValueType!.Type == ValueTypes.String || fields[j].OutputValueType!.Type == ValueTypes.QuotedCustom) && !rawData.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                    {
                        char quote = settings.DBMS!.StringChar;
                        string formattedData = $"{quote}{rawData.Replace(quote.ToString(), new string(quote, 2))}{quote}";

                        await writer.WriteAsync(formattedData);
                    }
                    else await writer.WriteAsync(rawData);


                    if (j + 1 < fields.Length) await writer.WriteAsync(',');
                }

                if (i < options.RecordsCount - 1) await writer.WriteLineAsync(settings.DBMS!.CompactInsert ? ")," : ");");
                else await writer.WriteLineAsync(");");
            }
        }

        public static async Task GenerateJSON(Stream stream, Field[] fields, GenerationOptions options)
        {
            JsonSettings settings = (JsonSettings)options.FormatSettings;
            if(settings.JsonLines)
            {
                byte[] newLineBytes = Encoding.UTF8.GetBytes("\n");
                for (ushort i = 0; i < options.RecordsCount; i++)
                {
                    var row = new Dictionary<string, object?>();
                    foreach (Field field in fields)
                    {
                        object? value = GenerateDataObject(field.OutputValueType!, field.Blank, i);
                        if (ReferenceEquals(value, Undefined.Value))
                            continue;
                        row.Add(field.Name!, value);
                    }

                    await JsonSerializer.SerializeAsync(stream, row, jsonLinesOptions);
                    await stream.WriteAsync(newLineBytes);
                }
            }
            else
            {
                var objects = new List<Dictionary<string, object?>>(options.RecordsCount);
                for (ushort i = 0; i < options.RecordsCount; i++)
                {
                    var row = new Dictionary<string, object?>();
                    foreach (Field field in fields)
                    {
                        object? value = GenerateDataObject(field.OutputValueType!, field.Blank, i);
                        if (ReferenceEquals(value, Undefined.Value))
                            continue;
                        row.Add(field.Name!, value);
                    }
                    objects.Add(row);
                }
                await JsonSerializer.SerializeAsync(stream, objects, jsonOptions);
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
                    ValueTypes.QuotedCustom => rules.DataFormats.GetValueOrDefault(fields[i].OutputValueType!.CustomTypeKey ?? "String", DBMSRules.SystemDefaults["String"]),
                    _ => throw new ArgumentOutOfRangeException(nameof(fields), "Неизвестный тип данных")
                };
                string fieldString = $"\t{rules.NameQuoteOpen}{fields[i].Name}{rules.NameQuoteClose} {formattedType}";

                if (fields[i].IsPK)
                    fieldString = $"{fieldString} PRIMARY KEY";

                if(!fields[i].IsPK && fields[i].IsNotNull)
                    fieldString = $"{fieldString} NOT NULL";

                if (!string.IsNullOrEmpty(fields[i].Check))
                    fieldString = $"{fieldString} CHECK({fields[i].Check})";

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
                if (fields[i].OutputValueType!.GenerationType == GenerationTypes.None)
                    continue;
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
                if (fields[i].OutputValueType!.GenerationType == GenerationTypes.None)
                    continue;
                string fieldName = $"{rules.NameQuoteOpen}{fields[i].Name}{rules.NameQuoteClose}";
                if (i + 1 < fields.Length)
                    fieldName += ",";
                await writer.WriteAsync(fieldName);
            }
            await writer.WriteAsync($") VALUES (");
        }

        private static string GenerateDataString(OutputValueType type, int BlankChance, ushort counter)
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
                    case GenerationTypes.None:
                        value = "Undefined";
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

        private static object? GenerateDataObject(OutputValueType type, int BlankChance, ushort counter)
        {
            object? value;
            if (BlankChance > 0)
            {
                bool isBlank = Random.Shared.NextSingle() < (Convert.ToSingle(BlankChance) / 100);
                if (isBlank) return null; 
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
                            value = number;
                        }
                        else if (type.Type == ValueTypes.Decimal)
                        {
                            double min = type.Data.TryGetValue("Min", out object? resultMin) ? Convert.ToDouble(resultMin) : 0;
                            double max = type.Data.TryGetValue("Max", out object? resultMax) ? Convert.ToDouble(resultMax) : 99;
                            decimal number = Convert.ToDecimal(Random.Shared.NextDouble() * (max - min) + min);
                            value = number;
                        }
                        else throw new Exception("Invalid Parameters");
                        break;
                    case GenerationTypes.Array:
                        var arrayData = (List<object>)(type.Data.GetValueOrDefault("Array"))! ?? throw new Exception("Invalid Array");
                        if (type.Type != ValueTypes.String) type.Type = ValueTypes.String;
                        value = arrayData[Random.Shared.Next(arrayData.Count)];
                        break;
                    case GenerationTypes.Increment:
                        int StartVal = Convert.ToInt32(type.Data.GetValueOrDefault("Start"));
                        int Step = Convert.ToInt32(type.Data.GetValueOrDefault("Step"));
                        value = (StartVal + (counter * Step));
                        break;
                    case GenerationTypes.Boolean:
                        bool data = Random.Shared.Next(2) == 0;
                        value = data;
                        break;
                    case GenerationTypes.Formula:
                        //Добавить проверку на ValueTypes 
                        //Парсинг строки кода JS из Data["Formula"]
                        value = type.GetFormulaOutput();
                        break;
                    case GenerationTypes.None:
                        value = Undefined.Value;
                        break;
                    default: throw new Exception("Invalid Generation Type");
                }
            }
            catch (Exception ex)
            {
                //Вывести ошибку в поле. В будущем заменить на логгирование.
                Debug.WriteLine(ex.Message);
                value = "invalid";
            }

            return value;
        }
    }
}

using Jint;
using Jint.Runtime;
using MockDataGenerator.Interfaces;
using MockDataGenerator.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MockDataGenerator.Models
{
    public class OutputValueType: IMockComponent
    {
        public required string DisplayName { get; set; }
        public required ValueTypes Type { get; set; }
        public required GenerationTypes GenerationType { get; set; }
        public required Dictionary<string, object> Data { get; set; }
        public string? CustomTypeKey { get; set; }
        [JsonIgnore]
        private Func<string>? _compiledFormula;
        public void Prepare()
        {
            if (GenerationType == GenerationTypes.Formula && Data.TryGetValue("Formula", out var formulaRaw))
            {
                string formula = formulaRaw.ToString()!;
                if (!formula.Contains("return") && !formula.Contains(';'))
                    formula = "return " + formula;
                try
                {
                    var compiledScript = Engine.PrepareScript(formula);
                    _compiledFormula = () =>
                    {
                        try
                        {
                            var del = FormulaService.engine.Evaluate(compiledScript).ToString();
                            return del;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            return "NULL";
                        }

                    };
                }
                catch (JavaScriptException jsEx)
                {
                    //Сообщить об ошибке в коде
                    Console.WriteLine(jsEx.Message);
                }
                catch (Exception ex)
                {
                    //Сообщить об ошибке
                    Console.WriteLine(ex.Message);
                }

            }
        }
        public string GetFormulaOutput()
        {
            if (GenerationType == GenerationTypes.Formula && _compiledFormula != null)
                return _compiledFormula.Invoke();
            else
                return string.Empty;
        }
    }
}

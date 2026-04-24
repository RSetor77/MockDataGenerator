using MockDataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.ViewModels
{
    public class OptionWindowViewModel: ViewModelBase
    {
        public Field EditField { get; set; } = new();
        public List<OptionItem> Parameters { get; set; } = [];

        public OptionWindowViewModel()
        {

        }
        public OptionWindowViewModel(Field field)
        {
            if (field == null) return;
            EditField = field;
            if (field.OutputValueType == null) return;
            if(EditField.OutputValueType!.GenerationType == GenerationTypes.Array)
            {
                if (EditField.OutputValueType.Data.GetValueOrDefault("Array") is object[] objects)

                for (ushort i = 0; i < objects.Length; i++)
                {
                    Parameters.Add(new() { Key = i.ToString(), Value = objects[i] });
                }
            } else
            {
                foreach(var item in EditField.OutputValueType!.Data)
                {
                    Parameters.Add(new() { Key = item.Key, Value = item.Value });
                }
            }
        }
    }
}

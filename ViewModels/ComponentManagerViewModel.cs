using Avalonia.Controls;
using Avalonia.Controls.Selection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MockDataGenerator.Models;
using MockDataGenerator.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockDataGenerator.ViewModels
{
    public partial class ComponentManagerViewModel : ViewModelBase
    {
        public event Action<Components?>? RequestClose;

        // Списки

        //Типы значений
        [ObservableProperty]
        private ObservableCollection<OutputValueType> _OutputValueTypes = [];
        public SelectionModel<OutputValueType> OVTSelection { get; } = new();
        //Правила БД
        [ObservableProperty]
        private ObservableCollection<DBMSRules> _dBMSRules = [];
        public SelectionModel<DBMSRules> DBMSRulesSelection { get; } = new();

        public ComponentManagerViewModel()
        {
            _ = InitComponents();
        }

        public async Task InitComponents()
        {
            try
            {
                var data = await ComponentService.GetComponentsAsync();
                OutputValueTypes = new(data);
            }
            catch (Exception) { OutputValueTypes = []; }
        }

        public async Task Refresh()
        {
            await InitComponents();
        }

#pragma warning disable CA1822 // Метод не может быть статическим
        public void OpenComponentFolder() => ComponentService.OpenComponentFolder();
#pragma warning restore CA1822 // Метод не может быть статическим

        public void ApplyAndClose()
        {
            var Output = new Components() { 
                OutputValueTypes = [.. OVTSelection.SelectedItems.Cast<OutputValueType>()],
                DBMSRules = [.. DBMSRulesSelection.SelectedItems!]
            };
            RequestClose?.Invoke(Output);
        }
    }
}

using Avalonia.Controls;
using Avalonia.Controls.Selection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MockDataGenerator.Interfaces;
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

        public ObservableCollection<IMockComponent> ComponentList { get; } = [];
        public SelectionModel<IMockComponent> Selection { get; } = new() { SingleSelect = false };

        [ObservableProperty]
        public int _currentTab;

        private byte _lastIndex = 0;

        partial void OnCurrentTabChanged(int value)
        {
            if (_lastIndex == 0)
                ComponentSelection.OutputValueTypes = [.. Selection.SelectedItems.OfType<OutputValueType>()];
            else if (_lastIndex == 1)
                ComponentSelection.DBMSRules = [.. Selection.SelectedItems.OfType<DBMSRules>()];

            SetList(value);

            _lastIndex = (byte)value;
        }

        private void SetList(int value)
        {
            if (value == 0)
            {
                UpdateList(OutputValueTypes);
            }
            else if (value == 1)
            {
                UpdateList(DBMSRules);
            }   
            else UpdateList([]);
        }

        private void UpdateList(IEnumerable<IMockComponent> list)
        {
            ComponentList.Clear();
            foreach(IMockComponent item in  list)
            {
                ComponentList.Add(item);
            }
        }

        //Типы значений
        public List<OutputValueType> OutputValueTypes { get; set; } = [];
        public List<DBMSRules> DBMSRules = [];

        public Components ComponentSelection { get; } = new();

        public ComponentManagerViewModel()
        {
            _ = Refresh();
        }

        public async Task InitComponents()
        {
            try
            {
                OutputValueTypes = [.. await ComponentService.GetComponentsAsync<OutputValueType>()];
                DBMSRules = [.. await ComponentService.GetComponentsAsync<DBMSRules>()];
            }
            catch (Exception) { 
                OutputValueTypes = [];
                DBMSRules = [];
            }
        }

        public async Task Refresh()
        {
            ComponentList.Clear();
            await InitComponents();
            SetList(CurrentTab);
        }

#pragma warning disable CA1822 // Метод не может быть статическим
        public void OpenComponentFolder() => ComponentService.OpenComponentFolder();
#pragma warning restore CA1822 // Метод не может быть статическим

        public void ApplyAndClose()
        {
            if (CurrentTab == 0)
                ComponentSelection.OutputValueTypes = [.. Selection.SelectedItems.OfType<OutputValueType>()];
            else
                ComponentSelection.DBMSRules = [.. Selection.SelectedItems.OfType<DBMSRules>()];

            RequestClose?.Invoke(ComponentSelection);
        }
    }
}

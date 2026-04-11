using Avalonia.Controls;
using MockDataGenerator.Models;
using MockDataGenerator.ViewModels;
using System;

namespace MockDataGenerator.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel;
        public MainWindow()
        {
            InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                if (_viewModel != null)
                {
                    _viewModel.RequestComponents -= OnOpenComponentManager;
                }

                if (DataContext is MainWindowViewModel viewModel)
                {
                    _viewModel = viewModel;
                    _viewModel.RequestComponents += OnOpenComponentManager;
                }
            };
        }

        private async void OnOpenComponentManager(Action<Components> callback)
        {
            ComponentManager manager = new();

            var result = await manager.ShowDialog<Components>(this);

            if (result != null)
            {
                callback(result);
            } else callback(new Components());
        }
    }
}
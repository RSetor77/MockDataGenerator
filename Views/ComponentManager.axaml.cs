using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MockDataGenerator.Models;
using MockDataGenerator.ViewModels;
using System;

namespace MockDataGenerator;

public partial class ComponentManager : Window
{
    private ComponentManagerViewModel? _viewModel;
    public ComponentManager()
    {
        InitializeComponent();
        _viewModel = new ComponentManagerViewModel();
        this.DataContext = _viewModel;

        _viewModel.RequestClose += OnRequestClose;
    }

    private void OnRequestClose(object? output)
    {
        this.Close(output);
    }
}
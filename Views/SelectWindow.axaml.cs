using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MockDataGenerator.Models;
using MockDataGenerator.ViewModels;

namespace MockDataGenerator;

public partial class SelectWindow : Window
{
    private readonly SelectWindowViewModel? _viewModel;
    public SelectWindow()
    {
        InitializeComponent();
        if(_viewModel != null)
        {
            _viewModel.RequestClose -= OnRequestClose;
        }

        if(this.DataContext is SelectWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.RequestClose += OnRequestClose;
        }
    }

    public SelectWindow(SelectWindowViewModel vm)
    {
        InitializeComponent();

        if (_viewModel != null)
        {
            _viewModel.RequestClose -= OnRequestClose;
        }

        if (vm is SelectWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.RequestClose += OnRequestClose;
        }
    }

    private async void OnRequestClose(OutputValueType? value)
    {
        this.Close(value);
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;

namespace MockDataGenerator;

public partial class ArraySettings : UserControl
{
    public ArraySettings()
    {
        InitializeComponent();
    }

    private void AddItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var list = ArrayList.ItemsSource as List<string>;
        list?.Add("");
    }
}
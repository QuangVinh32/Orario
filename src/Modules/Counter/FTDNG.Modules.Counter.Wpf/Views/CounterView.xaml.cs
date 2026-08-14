using System.Windows;
using System.Windows.Controls;

namespace FTDNG.Modules.Counter.Wpf.Views;

public partial class CounterView : UserControl
{
    private readonly CounterViewModel _viewModel;

    // ViewModel do DI cấp qua constructor — View không tự new.
    public CounterView(CounterViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void OnIncreaseClick(object sender, RoutedEventArgs e) => _viewModel.Increase();

    private void OnDecreaseClick(object sender, RoutedEventArgs e) => _viewModel.Decrease();

    private void OnResetClick(object sender, RoutedEventArgs e) => _viewModel.Reset();
}

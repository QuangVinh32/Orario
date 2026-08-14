using System.Windows;
using System.Windows.Controls;

namespace FTDNG.Modules.Sample.Wpf.Views;

public partial class SampleCanvasView : UserControl
{
    private readonly SampleViewModel _viewModel;

    // ViewModel được cấp qua DI (Host resolve view -> DI cấp dependency).
    public SampleCanvasView(SampleViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void OnRaiseClick(object sender, RoutedEventArgs e) => _viewModel.RaiseProjectOpened();

    private void OnAskClick(object sender, RoutedEventArgs e) => _viewModel.AskGreeting();
}

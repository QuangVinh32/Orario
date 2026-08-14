using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace FTDNG.Host.Wpf.Shell;

/// <summary>
/// Cửa sổ chính của Host. Chỉ là vỏ hiển thị: nhận ViewModel qua DI và ghép slot.
/// KHÔNG new ViewModel của module, KHÔNG chứa nghiệp vụ (Mục 4.2).
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = _viewModel;

        // Ghép contribution sau khi cửa sổ đã khởi tạo (đang ở UI thread).
        Loaded += (_, _) => _viewModel.Compose();
    }

    private void OnExitClick(object sender, RoutedEventArgs e) => Close();

    /// <summary>
    /// Handler chung cho các nút header. Chỉ báo trạng thái ra StatusBar —
    /// KHÔNG xử lý nghiệp vụ tại Host (Mục 4.2). Nghiệp vụ thật sẽ do module
    /// đăng ký command qua UiRegistry đảm nhận.
    /// </summary>
    private void OnHeaderCommand(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element) return;

        var id = element.Tag?.ToString() ?? "?";
        var state = sender is ToggleButton { IsChecked: var on }
            ? (on == true ? " (bật)" : " (tắt)")
            : string.Empty;

        _viewModel.StatusText = $"Lệnh header: {id}{state}";
    }

    /// <summary>Ẩn nút overflow (mũi tên ▾) ở cuối ToolBar.</summary>
    private void OnToolBarLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ToolBar toolBar) return;

        if (toolBar.Template.FindName("OverflowGrid", toolBar) is FrameworkElement overflow)
            overflow.Visibility = Visibility.Collapsed;

        if (toolBar.Template.FindName("MainPanelBorder", toolBar) is FrameworkElement mainPanel)
            mainPanel.Margin = new Thickness(0);
    }

    /// <summary>Lăn chuột dọc để cuộn toolbar theo chiều ngang.</summary>
    private void OnToolbarWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer sv) return;
        sv.ScrollToHorizontalOffset(sv.HorizontalOffset - e.Delta);
        e.Handled = true;
    }
}

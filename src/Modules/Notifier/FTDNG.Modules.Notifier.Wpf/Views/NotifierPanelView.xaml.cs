using System.Windows.Controls;

namespace FTDNG.Modules.Notifier.Wpf.Views;

public partial class NotifierPanelView : UserControl
{
    private readonly NotifierViewModel _viewModel;

    // ViewModel do DI cấp — chính nó đã Subscribe EventBus trong constructor.
    public NotifierPanelView(NotifierViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnSendPingClick(object sender, System.Windows.RoutedEventArgs e) => _viewModel.SendPing();
}

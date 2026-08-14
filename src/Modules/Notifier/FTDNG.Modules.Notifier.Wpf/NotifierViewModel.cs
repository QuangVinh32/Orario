using System.Collections.ObjectModel;
using System.ComponentModel;
using FTDNG.Contracts.Events;

namespace FTDNG.Modules.Notifier.Wpf;

/// <summary>
/// ViewModel của Notifier. Đăng ký nghe ProjectOpenedEvent trên EventBus của Host.
/// Khi Sample (hoặc bất kỳ module nào) phát event, Notifier ghi một dòng log —
/// chứng minh giao tiếp xuyên module không cần biết nhau.
/// </summary>
public sealed class NotifierViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IEventBus _eventBus;
    private readonly IDisposable _subscription;
    private int _pingNo;

    public NotifierViewModel(IEventBus eventBus)
    {
        _eventBus = eventBus;
        // Nghe tin "dự án vừa được mở" — không quan tâm AI phát ra.
        _subscription = eventBus.Subscribe<ProjectOpenedEvent>(OnProjectOpened);
    }

    /// <summary>Chiều ngược: Notifier PHÁT NotifierPingEvent; ai nghe (vd Sample) thì tự xử lý.</summary>
    public void SendPing()
    {
        _pingNo++;
        _eventBus.Publish(new NotifierPingEvent($"Ping #{_pingNo} lúc {DateTime.Now:HH:mm:ss}"));
    }

    /// <summary>Danh sách log hiển thị trên UI (mới nhất nằm trên đầu).</summary>
    public ObservableCollection<string> Logs { get; } = new()
    {
        "Notifier sẵn sàng. Đang chờ ProjectOpenedEvent từ module khác..."
    };

    private int _count;
    public int Count
    {
        get => _count;
        private set { _count = value; OnPropertyChanged(nameof(Count)); }
    }

    private void OnProjectOpened(ProjectOpenedEvent e)
    {
        Count++;
        Logs.Insert(0, $"[{DateTime.Now:HH:mm:ss}] #{Count} nhận ProjectOpenedEvent: {e.ProjectId}");
    }

    public void Dispose() => _subscription.Dispose();

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

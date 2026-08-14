using System.ComponentModel;
using FTDNG.Contracts.Events;
using FTDNG.Contracts.Queries;
using FTDNG.SharedKernel;

namespace FTDNG.Modules.Sample.Wpf;

/// <summary>
/// ViewModel demo. Nghiệp vụ (giả lập) chạy TRONG module — Host không can thiệp.
/// Minh họa 2 kênh giao tiếp do Host cung cấp:
///  • IEventBus  — "báo tin" (publish/subscribe, nhiều người nghe).
///  • IQueryBus  — "hỏi-lấy dữ liệu ngay" (một nguồn trả lời, có kết quả).
/// </summary>
public sealed class SampleViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IEventBus _eventBus;
    private readonly IQueryBus _queryBus;
    private readonly IDisposable _subscription;    // hủy đăng ký ProjectOpenedEvent
    private readonly IDisposable _pingSubscription; // hủy đăng ký NotifierPingEvent
    private readonly IDisposable _greetingHandler;  // hủy đăng ký QueryBus

    public SampleViewModel(IEventBus eventBus, IQueryBus queryBus)
    {
        _eventBus = eventBus;
        _queryBus = queryBus;

        // (EventBus) Lắng nghe event do chính module phát ra để cập nhật UI.
        _subscription = _eventBus.Subscribe<ProjectOpenedEvent>(OnProjectOpened);

        // (Chiều ngược) Lắng nghe tin do MODULE NOTIFIER phát ra — Sample không biết Notifier là ai.
        _pingSubscription = _eventBus.Subscribe<NotifierPingEvent>(OnNotifierPing);

        // (QueryBus) Đăng ký MÌNH là nguồn trả lời cho GetGreetingQuery.
        // Trong thực tế "người trả lời" thường là MỘT module khác sở hữu dữ liệu;
        // ở đây gộp vào một module cho dễ chạy demo.
        _greetingHandler = _queryBus.RegisterHandler<GetGreetingQuery, string>(
            q => $"Xin chào {q.Name}! Bây giờ là {DateTime.Now:HH:mm:ss}.");
    }

    private string _lastMessage = "Chưa có sự kiện nào.";
    public string LastMessage
    {
        get => _lastMessage;
        private set { _lastMessage = value; OnPropertyChanged(nameof(LastMessage)); }
    }

    private string _queryResult = "Chưa hỏi lần nào.";
    public string QueryResult
    {
        get => _queryResult;
        private set { _queryResult = value; OnPropertyChanged(nameof(QueryResult)); }
    }

    private string _pingFromNotifier = "Chưa nhận Ping nào từ Notifier.";
    public string PingFromNotifier
    {
        get => _pingFromNotifier;
        private set { _pingFromNotifier = value; OnPropertyChanged(nameof(PingFromNotifier)); }
    }

    /// <summary>Nghiệp vụ demo (EventBus): phát một ProjectOpenedEvent lên bus.</summary>
    /// 
    public void RaiseProjectOpened()
    {
        var id = ProjectId.New();
        _eventBus.Publish(new ProjectOpenedEvent(id));
    }

    /// <summary>
    /// Nghiệp vụ demo (QueryBus): đóng vai "module cần dữ liệu" — hỏi rồi lấy kết quả ngay.
    /// TryAsk trả false nếu chưa có nguồn nào đăng ký (app vẫn chạy bình thường).
    /// </summary>
    //b2
    public void AskGreeting()
    {
        if (_queryBus.TryAsk<GetGreetingQuery, string>(new GetGreetingQuery("Vinh"), out var answer))
            QueryResult = $"Có nguồn trả lời → {answer}";
        else
            QueryResult = "Không có nguồn nào đăng ký trả lời query này.";
    }
    // b4
    private void OnProjectOpened(ProjectOpenedEvent e) =>
        LastMessage = $"Nhận ProjectOpenedEvent: {e.ProjectId}";

    // Sample phản ứng khi Notifier phát tin — chiều ngược lại.
    private void OnNotifierPing(NotifierPingEvent e) =>
        PingFromNotifier = $"Nhận từ Notifier → {e.Text}";

    public void Dispose()
    {
        _subscription.Dispose();
        _pingSubscription.Dispose();
        _greetingHandler.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

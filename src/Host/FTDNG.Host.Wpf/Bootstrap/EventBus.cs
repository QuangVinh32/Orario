using System.Collections.Concurrent;
using FTDNG.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace FTDNG.Host.Wpf.Bootstrap;

/// <summary>
/// Implementation EventBus tối giản, in-process, thread-safe (Mục 4.1).
/// Đặt tại Host theo baseline Beta 0.0.0 và đăng ký singleton qua DI.
/// </summary>
public sealed class EventBus : IEventBus
{
    // Khóa theo kiểu event -> tập handler. Dùng list được bảo vệ bằng lock để publish an toàn.
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private readonly ILogger<EventBus>? _logger;

    public EventBus(ILogger<EventBus>? logger = null) => _logger = logger;

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        var list = _handlers.GetOrAdd(typeof(TEvent), static _ => new List<Delegate>());
        lock (list)
        {
            list.Add(handler);
        }
        return new Subscription(() =>
        {
            lock (list)
            {
                list.Remove(handler);
            }
        });
    }

    public void Publish<TEvent>(TEvent message)
    {
        if (!_handlers.TryGetValue(typeof(TEvent), out var list))
            return;

        // Sao chép nhanh trong lock để tránh giữ lock khi gọi handler (handler có thể subscribe/unsubscribe).
        Delegate[] snapshot;
        lock (list)
        {
            snapshot = list.ToArray();
        }

        foreach (var d in snapshot)
        {
            try
            {
                ((Action<TEvent>)d).Invoke(message);
            }
            catch (Exception ex)
            {
                // Một handler lỗi không được làm hỏng cả chuỗi publish.
                _logger?.LogError(ex, "Handler cho event {EventType} ném exception.", typeof(TEvent).Name);
            }
        }
    }

    private sealed class Subscription(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose;
        public void Dispose()
        {
            _dispose?.Invoke();
            _dispose = null;
        }
    }
}

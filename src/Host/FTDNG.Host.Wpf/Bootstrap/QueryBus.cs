using System.Collections.Concurrent;
using FTDNG.Contracts.Queries;
using Microsoft.Extensions.Logging;

namespace FTDNG.Host.Wpf.Bootstrap;

/// <summary>
/// Implementation QueryBus tối giản, in-process, thread-safe (Mục 4.1, đối xứng với EventBus).
/// Đặt tại Host theo baseline Beta 0.0.0 và đăng ký singleton qua DI.
/// Mỗi cặp (query, result) chỉ có MỘT handler — query có một nguồn dữ liệu duy nhất.
/// </summary>
public sealed class QueryBus : IQueryBus
{
    // Khóa theo cặp (kiểu query, kiểu result) -> handler duy nhất.
    private readonly ConcurrentDictionary<(Type Query, Type Result), Delegate> _handlers = new();
    private readonly ILogger<QueryBus>? _logger;

    public QueryBus(ILogger<QueryBus>? logger = null) => _logger = logger;

    public IDisposable RegisterHandler<TQuery, TResult>(Func<TQuery, TResult> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        var key = (typeof(TQuery), typeof(TResult));
        if (!_handlers.TryAdd(key, handler))
        {
            throw new InvalidOperationException(
                $"Đã có handler cho query {typeof(TQuery).Name} -> {typeof(TResult).Name}. " +
                "Mỗi query chỉ được có một nguồn dữ liệu.");
        }

        return new Registration(() =>
            _handlers.TryRemove(new KeyValuePair<(Type, Type), Delegate>(key, handler)));
    }

    public bool TryAsk<TQuery, TResult>(TQuery query, out TResult result)
    {
        if (_handlers.TryGetValue((typeof(TQuery), typeof(TResult)), out var handler))
        {
            try
            {
                result = ((Func<TQuery, TResult>)handler).Invoke(query);
                return true;
            }
            catch (Exception ex)
            {
                // Handler lỗi không được làm sập lời gọi hỏi dữ liệu; coi như không có kết quả.
                _logger?.LogError(ex, "Handler cho query {QueryType} ném exception.", typeof(TQuery).Name);
            }
        }

        result = default!;
        return false;
    }

    private sealed class Registration(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose;
        public void Dispose()
        {
            _dispose?.Invoke();
            _dispose = null;
        }
    }
}

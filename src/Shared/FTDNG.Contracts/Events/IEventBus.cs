namespace FTDNG.Contracts.Events;

/// <summary>
/// EventBus tối giản để module phát/nhận "một việc đã xảy ra" mà không reference chéo (Mục 4.1).
/// Không dùng EventBus để hỏi-lấy dữ liệu ngay — việc đó dùng query/service interface.
/// </summary>
public interface IEventBus
{
    /// <summary>Đăng ký handler cho <typeparamref name="TEvent"/>. Dispose để hủy đăng ký.</summary>
    IDisposable Subscribe<TEvent>(Action<TEvent> handler);

    /// <summary>Phát một event tới mọi subscriber hiện tại của <typeparamref name="TEvent"/>.</summary>
    void Publish<TEvent>(TEvent message);
}

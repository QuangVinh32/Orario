namespace FTDNG.Contracts.Queries;

/// <summary>
/// Kênh "hỏi-lấy dữ liệu ngay" giữa các module mà không reference chéo (Mục 4.1, đối xứng với IEventBus).
/// Dùng IQueryBus khi cần đọc dữ liệu đồng bộ; dùng IEventBus khi chỉ báo "một việc đã xảy ra".
/// Khác EventBus (nhiều subscriber), mỗi loại query chỉ có MỘT nguồn dữ liệu (một handler).
/// UI-neutral: chỉ mang query/result là DTO, KHÔNG mang WPF View/Control.
/// </summary>
public interface IQueryBus
{
    /// <summary>
    /// Module sở hữu dữ liệu đăng ký handler trả lời cho <typeparamref name="TQuery"/>.
    /// Dispose để hủy đăng ký. Ném nếu đã có handler cho cùng cặp query/result.
    /// </summary>
    IDisposable RegisterHandler<TQuery, TResult>(Func<TQuery, TResult> handler);

    /// <summary>
    /// Module cần dữ liệu hỏi một <typeparamref name="TQuery"/>.
    /// Trả về <c>false</c> nếu chưa có nguồn nào đăng ký (slot dữ liệu trống mà app vẫn chạy).
    /// </summary>
    bool TryAsk<TQuery, TResult>(TQuery query, out TResult result);
}

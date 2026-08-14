namespace FTDNG.Contracts.Queries;

/// <summary>
/// Query demo cho <see cref="IQueryBus"/>: "hỏi lời chào theo tên", kết quả là <c>string</c>.
/// Đặt ở Contracts để module HỎI và module TRẢ LỜI cùng "hiểu" một kiểu mà không reference chéo.
/// (Trong thực tế đây sẽ là những query như GetCurrentProject, GetBarCount…)
/// </summary>
public sealed record GetGreetingQuery(string Name);

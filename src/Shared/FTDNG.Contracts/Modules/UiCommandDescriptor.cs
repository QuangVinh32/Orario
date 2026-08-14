namespace FTDNG.Contracts.Modules;

/// <summary>
/// Mô tả một Command mà module đóng góp vào menu/toolbar của Host.
/// UI-neutral: chỉ mang metadata + delegate; KHÔNG mang WPF control.
/// </summary>
public sealed class UiCommandDescriptor
{
    /// <summary>Định danh ổn định của command trong phạm vi module (ví dụ "calendar.open").</summary>
    public required string Id { get; init; }

    /// <summary>Nhãn hiển thị cho người dùng (ví dụ "Calendar...").</summary>
    public required string Title { get; init; }

    /// <summary>Hành động thực thi khi command được kích hoạt. Nghiệp vụ chạy trong module.</summary>
    public required Action Execute { get; init; }

    /// <summary>Điều kiện cho phép thực thi (mặc định luôn cho phép).</summary>
    public Func<bool>? CanExecute { get; init; }

    /// <summary>Thứ tự sắp xếp trong cùng một location (nhỏ hơn hiển thị trước).</summary>
    public int Order { get; init; }

    /// <summary>Gợi ý phím tắt dạng text (tùy chọn, ví dụ "Ctrl+K").</summary>
    public string? InputGestureText { get; init; }
}

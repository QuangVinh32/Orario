namespace FTDNG.SharedKernel;

/// <summary>
/// Các định danh mạnh (strongly-typed IDs) dùng chung, cực kỳ ổn định và UI-free.
/// Dùng readonly record struct để so sánh theo giá trị mà không cấp phát heap.
/// Chi tiết vòng đời/nghiệp vụ thuộc về module sở hữu; SharedKernel chỉ giữ primitive.
/// </summary>
public readonly record struct ProjectId(Guid Value)
{
    public static ProjectId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("D");
}

public readonly record struct CalendarId(Guid Value)
{
    public static CalendarId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("D");
}

public readonly record struct TaskRowId(Guid Value)
{
    public static TaskRowId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("D");
}

public readonly record struct BarId(Guid Value)
{
    public static BarId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("D");
}

public readonly record struct DependencyId(Guid Value)
{
    public static DependencyId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("D");
}

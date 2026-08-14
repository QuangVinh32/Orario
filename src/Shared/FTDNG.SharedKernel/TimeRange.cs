namespace FTDNG.SharedKernel;

/// <summary>
/// Khoảng thời gian bắt đầu - kết thúc (half-open: [Start, End)). Primitive UI-free.
/// </summary>
public readonly record struct TimeRange
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    public TimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        if (end < start)
            throw new ArgumentException("End không được nhỏ hơn Start.", nameof(end));
        Start = start;
        End = end;
    }

    public TimeSpan Duration => End - Start;

    public bool Contains(DateTimeOffset moment) => moment >= Start && moment < End;

    public bool Overlaps(TimeRange other) => Start < other.End && other.Start < End;
}

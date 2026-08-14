namespace FTDNG.SharedKernel;

/// <summary>
/// Lỗi nghiệp vụ dạng dữ liệu (không ném exception cho luồng bình thường).
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}

/// <summary>
/// Result không mang giá trị: thành công hoặc thất bại kèm <see cref="Error"/>.
/// </summary>
public readonly struct Result
{
    public bool IsSuccess { get; }
    public Error Error { get; }
    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
}

/// <summary>
/// Result mang giá trị <typeparamref name="T"/> khi thành công.
/// </summary>
public readonly struct Result<T>
{
    private readonly T? _value;

    public bool IsSuccess { get; }
    public Error Error { get; }
    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, T? value, Error error)
    {
        IsSuccess = isSuccess;
        _value = value;
        Error = error;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Không thể lấy Value của một Result thất bại.");

    public static Result<T> Success(T value) => new(true, value, Error.None);
    public static Result<T> Failure(Error error) => new(false, default, error);
}

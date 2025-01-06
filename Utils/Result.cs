namespace Boto.Utils;

#pragma warning disable CA1000

public enum ErrType
{
    None,
    InvalidInput,
    AccessDenied,
    Timeout,
    NetworkError,
    ProgramError,
    UnknownError,
}

public readonly struct Err(ErrType errorType, string errorMessage)
{
    public string ErrorMessage { get; } = errorMessage;
    public ErrType ErrorType { get; } = errorType;
}

public readonly struct Result<T>
{
    private bool _isSuccess { get; init; }
    public readonly bool IsOk => _isSuccess;
    public T? Value { get; init; }
    public Err Err { get; init; }

    private Result(T value)
    {
        _isSuccess = true;
        Value = value;
        Err = default;
    }

    private Result(Err err)
    {
        _isSuccess = false;
        Value = default;
        Err = err;
    }

    public static Result<T> Ok(T value) => new(value);

    public static Result<T> Failure(Err err) => new(err);

    public static implicit operator Result<T>(T value) => Ok(value);

    public static implicit operator Result<T>(Err err) => Failure(err);

    public TResult Match<TResult>(Func<T?, TResult> success, Func<Err, TResult> failure) =>
        _isSuccess ? success(Value) : failure(Err);
}

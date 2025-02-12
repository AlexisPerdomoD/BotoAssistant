using System.Net.NetworkInformation;
using System.Text.Json;

namespace Boto.Utils;

#pragma warning disable CA1000
public readonly struct None { }

public enum ErrType
{
    None,
    InvalidInput,
    AccessDenied,
    Timeout,
    NetworkError,
    ExternalError,
    ProgramError,
    UnknownError,
}

public readonly struct Err
{
    public string Message { get; }
    public ErrType Type { get; }

    private Err(ErrType type, string message)
    {
        Type = type;
        Message = message;
    }

    public static Err InvalidInput(string message) => new(ErrType.InvalidInput, message);

    public static Err AccessDenied(string message) => new(ErrType.AccessDenied, message);

    public static Err Timeout(string message) => new(ErrType.Timeout, message);

    public static Err NetworkError(string message) => new(ErrType.NetworkError, message);

    public static Err ExternalError(string message) => new(ErrType.ExternalError, message);

    public static Err ProgramError(string message) => new(ErrType.ProgramError, message);

    public static Err UnknownError(string message) => new(ErrType.UnknownError, message);
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

    public static Result<TArg> Ok<TArg>(TArg value) => new(value);

    public static Result<T> Failure(Err err) => new(err);

    public static implicit operator Result<T>(T value) => Ok(value);

    public static implicit operator Result<T>(Err err) => Failure(err);

    public TResult Match<TResult>(Func<T?, TResult> success, Func<Err, TResult> failure) =>
        _isSuccess ? success(Value) : failure(Err);

    public static async Task<Result<T>> FromTask(Task<T> task)
    {
        try
        {
            var value = await task;
            return Ok(value);
        }
        catch (Exception e)
        {
            return e switch
            {
                JsonException => Err.InvalidInput(e.Message),
                HttpRequestException => Err.NetworkError(e.Message),
                NetworkInformationException => Err.NetworkError(e.Message),
                FileLoadException => Err.ProgramError(e.Message),
                _ => Err.UnknownError(e.Message),
            };
        }
    }

    public static async Task<Result<None>> FromTask(Task task)
    {
        try
        {
            await task;
            return Ok(new None());
        }
        catch (Exception e)
        {
            return e switch
            {
                JsonException => Err.InvalidInput(e.Message),
                HttpRequestException => Err.NetworkError(e.Message),
                NetworkInformationException => Err.NetworkError(e.Message),
                FileLoadException => Err.ProgramError(e.Message),
                _ => Err.UnknownError(e.Message),
            };
        }
    }
}

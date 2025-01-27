using Boto.interfaces;
using Microsoft.Extensions.Logging;

namespace Boto.Utils;

public abstract class MainIOMannager(LogLevel logLevel) : IIOMannagerService
{
    private readonly LogLevel _logLevel = logLevel;
    public string? LastInput { get; protected set; }
    public List<string> History { get; private set; } = [];

    public virtual string? GetInput(
        string prompt,
        Func<string?, bool>? validator,
        string? customTryAgainMessage
    )
    {
        validator ??= static i => true;
        this.LogInformation(prompt, false);
        var input = Console.ReadLine();
        var tries = 0;
        if (string.IsNullOrWhiteSpace(input))
            return null;
        while (true)
        {
            input = input.Trim().ToLowerInvariant();

            if (validator(input))
                break;

            if (++tries > 2)
            {
                this.LogWarning("Too many tries...");
                return null;
            }

            this.LogWarning(customTryAgainMessage ?? "Invalid input. Try again.");
            input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return null;
        }

        LastInput = input;
        History.Add(input);
        return input;
    }

    public virtual void WaitInteraction(bool clearScreen)
    {
        LogInformation("\n\n Press enter key to continue...\n");
        _ = Console.ReadLine();
        if (clearScreen)
            Console.Clear();
        return;
    }

    public virtual void WaitInteraction()
    {
        LogInformation("\n\n Press enter key to continue...\n");
        _ = Console.ReadLine();
        return;
    }

    public virtual void ClearHistory()
    {
        this.History.Clear();
        this.LastInput = null;
    }

    private string _defaultFormater(string message, Exception? exception)
    {
        if (exception != null)
        {
            string exceptionMessage = message != "" ? $"{message}\n" : "";
            exceptionMessage +=
                $"Exception happend at {DateTime.Now:yyyy-MM-dd HH:mm:ss} | {exception.Message}\n{exception.StackTrace}";
            return exceptionMessage;
        }
        return $"{message}";
    }

    public virtual bool IsEnabled(LogLevel logLevel) => logLevel >= this._logLevel;

    // Para simplificar, no implementamos un scope. Si lo necesitas, puedes devolver un objeto IDisposable.
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull => null!;

    public virtual void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        if (!this.IsEnabled(logLevel))
            return;

        string logMessage;
        logMessage = formatter(state, exception);
        Console.WriteLine(logMessage);
    }

    public virtual void LogInformation(string message, bool? time = false)
    {
        string logMessage =
            time == true ? $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}" : message;
        this.Log(LogLevel.Information, default, logMessage, null, this._defaultFormater);
    }

    public virtual void LogWarning(string message, bool? time = false)
    {
        string logMessage =
            time == true ? $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}" : message;
        this.Log(LogLevel.Warning, default, logMessage, null, this._defaultFormater);
    }

    public virtual void LogError(string message, Exception e) =>
        this.Log(LogLevel.Error, default, message, e, this._defaultFormater);

    public virtual void LogCritical(string message, Exception e) =>
        this.Log(LogLevel.Critical, default, message, e, this._defaultFormater);

    public virtual void LogDebug(string message) =>
        this.Log(LogLevel.Debug, default, message, null, this._defaultFormater);

    public virtual void ClearLogs() => Console.Clear();

    public abstract string? GetInputSelector(
        string prompt,
        string[] options,
        Func<string?, bool>? validator = null,
        string? customTryAgainMessage = null
    );
}

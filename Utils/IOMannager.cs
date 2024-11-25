using Microsoft.Extensions.Logging;
using Boto.Models;

namespace Boto.Utils;
public class MainIOMannager(LogLevel logLevel) : IBotoLogger, IInputOutputMannager
{
    private readonly LogLevel _logLevel = logLevel;
    public string? LastInput { get; private set; }
    public List<string> History { get; private set; } = [];

    public string? GetInput(string prompt, Func<string?, bool>? validator = null, string? customTryAgainMessage = null)
    {
        validator ??= static i => !string.IsNullOrWhiteSpace(i);
        this.LogInformation(prompt, false);
        string? input = Console.ReadLine();
        int tries = 0;
        while (!validator(input))
        {
            this.LogInformation(customTryAgainMessage ?? "Invalid input. Please try again.", false);
            input = Console.ReadLine();
            tries++;
            if (tries > 3)
            {
                this.LogInformation("Too many tries. Ending selection.", true);
                input = null;
                break;
            }
        }
        if (input is not null)
        {
            this.LastInput = input;
            this.History.Add(input);
        }
        return input;
    }
    public void ClearHistory()
    {
        this.History.Clear();
        this.LastInput = null;
    }

    private string _defaultFormater(string message, Exception? exception)
    {
        if (exception != null)
        {
            string exceptionMessage = message != "" ? $"{message}\n" : "";
            exceptionMessage += $"Exception happend at {DateTime.Now:yyyy-MM-dd HH:mm:ss} | {exception.Message}\n{exception.StackTrace}";
            return exceptionMessage;

        }
        return $"{message}";
    }
    public bool IsEnabled(LogLevel logLevel) => logLevel >= this._logLevel;
    // Para simplificar, no implementamos un scope. Si lo necesitas, puedes devolver un objeto IDisposable.
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!this.IsEnabled(logLevel)) return;

        string logMessage;
        logMessage = formatter(state, exception);
        Console.WriteLine(logMessage);
    }

    public void LogInformation(string message, bool? time = false)
    {
        string logMessage = time == true ? $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}" : message;
        this.Log(LogLevel.Information, default, logMessage, null, this._defaultFormater);
    }

    public void LogWarning(string message, bool? time = false)
    {
        string logMessage = time == true ? $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}" : message;
        this.Log(LogLevel.Warning, default, logMessage, null, this._defaultFormater);
    }
    public void LogError(string message, Exception e)
        => this.Log(LogLevel.Error, default, message, e, this._defaultFormater);
    public void LogCritical(string message, Exception e)
        => this.Log(LogLevel.Critical, default, message, e, this._defaultFormater);
    public void LogDebug(string message)
        => this.Log(LogLevel.Debug, default, message, null, this._defaultFormater);
    public void ClearLogs()
        => Console.Clear();

}

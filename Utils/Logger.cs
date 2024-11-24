using Microsoft.Extensions.Logging;
namespace Boto.Utils;



public enum LogCategory
{
    ExternalError,
    InternalError,
    Information,
    Warning,
    Error,
    Critical,
    Debug,
}

public interface IBotoLogger : ILogger
{
    public void LogDebug(string message);
    public void LogInformation(string message, bool? time);
    public void LogWarning(string message, bool? time);
    public void LogError(string message, Exception e);
    public void LogCritical(string message, Exception e);
}
public class BotoMainLogger(LogLevel logLevel) : IBotoLogger
{
    private readonly LogLevel _logLevel = logLevel;
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


    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;
    // Para simplificar, no implementamos un scope. Si lo necesitas, puedes devolver un objeto IDisposable.
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!this.IsEnabled(logLevel)) return;

        string logMessage;
        logMessage = formatter(state, exception);
        Console.WriteLine(logMessage);
    }

    public void LogInformation(string message, bool? time)
    {
        string logMessage = time == true ? $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}" : message;
        this.Log(LogLevel.Information, default, logMessage, null, this._defaultFormater);
    }

    public void LogWarning(string message, bool? time)
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


}

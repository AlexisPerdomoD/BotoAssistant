namespace Boto.Utils;

using Microsoft.Extensions.Logging;

enum LogCategory
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
    public void LogDebug(string m);
    public void LogInformation(string m, bool? time);
    public void LogWarning(string m, bool? time);
    public void LogError(string m, Exception e);
    public void LogCritical(string m, Exception e);
}
public class BotoMainLogger : IBotoLogger
{
    private readonly LogLevel _logLevel;
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


    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        // Para simplificar, no implementamos un scope. Si lo necesitas, puedes devolver un objeto IDisposable.
        return null!;
    }
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        string logMessage;
        logMessage = formatter(state, exception);
        Console.WriteLine(logMessage);
    }

    public BotoMainLogger(LogLevel logLevel)
    {
        _logLevel = logLevel;
    }

    public void LogInformation(string message, bool? includeTime)
    {
        string logMessage = includeTime == true ? $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}" : message;
        Log(LogLevel.Information, default, logMessage, null, _defaultFormater);
    }

    public void LogWarning(string message, bool? includeTime)
    {
        string logMessage = includeTime == true ? $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}" : message;
        Log(LogLevel.Warning, default, logMessage, null, _defaultFormater);
    }

    public void LogError(string message, Exception exception)
    {
        Log(LogLevel.Error, default, message, exception, _defaultFormater);
    }

    public void LogCritical(string message, Exception exception)
    {
        Log(LogLevel.Critical, default, message, exception, _defaultFormater);
    }

    public void LogDebug(string message)
    {
        Log(LogLevel.Debug, default, message, null, _defaultFormater);
    }

}

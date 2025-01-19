using Microsoft.Extensions.Logging;

namespace Boto.interfaces;

public interface IChat
{
    string? Current { get; set; }
    Dictionary<string, string> History { get; }
}

/// <summary>
///   Interface for a logger that can be used by the main program
/// </summary>
/// <remarks>
///   A logger is a class that implements this interface and has a LogLevel
/// </remarks>
public interface IBotoLogger : ILogger
{
    void LogDebug(string message);
    void LogInformation(string message, bool? time = false);
    void LogWarning(string message, bool? time = false);
    void LogError(string message, Exception e);
    void LogCritical(string message, Exception e);
    void ClearLogs();
}

/// <summary>
///   Interface for a input/output manager that can be used by the main program
/// </summary>
/// <remarks>
///   An input/output manager is able to get user input and manage the history of the input
/// </remarks>
public interface IInputOutputMannager
{
    string? GetInput(
        string prompt,
        Func<string?, bool>? validator = null,
        string? customTryAgainMessage = null
    );
    void ClearHistory();
    void WaitInteraction();
    void WaitInteraction(bool clearScreen);
    string? LastInput { get; }
    List<string> History { get; }
}

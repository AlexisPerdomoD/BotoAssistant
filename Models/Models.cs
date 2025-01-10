using Microsoft.Extensions.Logging;

namespace Boto.Models;

public interface IChat<T>
{
    string? Current { get; set; }
    List<T> History { get; }
}

/// --- Main Models ---

/// <summary>
///   Interface for a user that can be used by the main program
/// </summary>
/// <remarks>
///   A user is a class that implements this interface and has a Name, a Profile and a list of Tags
/// </remarks>
public interface IUsr
{
    string Name { get; }
    string UsrProfile { get; set; }
    DateTime LastLogin { get; set; }
    string[] ProfileTags { get; set; }

    /// <summary>
    ///   Method to load the user's STS data, usually on "${BOTO_WORKING_DIRECTORY}/usr/usr.json"
    /// </summary>
    /// <param name="usr"></param>
    /// <returns>error message if any, else null</returns>
    Task<string?> SaveUsrSts();
}

/// <summary>
///   Interface for a user manager that can be used by the main program
/// </summary>
/// <remarks>
///   A user manager is able to verify if a user exists, create a new user and get the current user
/// </remarks>
public interface IUsrMannager
{
    Task<(Exception? e, IUsr? usr)> UsrExists(string usrName);
    Task<(Exception? e, IUsr? usr)> CreateUsr(
        string usrName,
        string usrProfile,
        string[] profileTags
    );
    IUsr? GetCurrentUsr();
    Task<bool> SetCurrentUsr(IUsr? usr);
}

/// --- Utils Models ---

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
    string? LastInput { get; }
    List<string> History { get; }
}

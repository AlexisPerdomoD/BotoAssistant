using Microsoft.Extensions.Logging;

namespace Boto.Models;

public interface IChat<T>
{
    public string? Current { get; set; }
    public List<T> History { get; }
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
    public string Name { get; }
    public string UsrProfile { get; set; }
    public DateTime LastLogin { get; set; }
    public string[] ProfileTags { get; set; }

    /// <summary>
    ///   Method to load the user's STS data, usually on "${BOTO_WORKING_DIRECTORY}/usr/usr.json"
    /// </summary>
    /// <param name="usr"></param>
    /// <returns>error message if any, else null</returns>
    public Task<string?> SaveUsrSts();
}

/// <summary>
///   Interface for a user manager that can be used by the main program
/// </summary>
/// <remarks>
///   A user manager is able to verify if a user exists, create a new user and get the current user
/// </remarks>
public interface IUsrMngr
{
    public Task<(Exception? e, IUsr? usr)> UsrExists(string usrName);
    public Task<(Exception? e, IUsr? usr)> CreateUsr(
        string usrName,
        string usrProfile,
        string[] profileTags
    );
    public IUsr? GetCurrentUsr();
    public Task<bool> SetCurrentUsr(IUsr? usr);
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
    public void LogDebug(string message);
    public void LogInformation(string message, bool? time = false);
    public void LogWarning(string message, bool? time = false);
    public void LogError(string message, Exception e);
    public void LogCritical(string message, Exception e);
    public void ClearLogs();
}

/// <summary>
///   Interface for a input/output manager that can be used by the main program
/// </summary>
/// <remarks>
///   An input/output manager is able to get user input and manage the history of the input
/// </remarks>
public interface IInputOutputMannager
{
    public string? GetInput(
        string prompt,
        Func<string?, bool>? validator = null,
        string? customTryAgainMessage = null
    );
    public void ClearHistory();
    public string? LastInput { get; }
    public List<string> History { get; }
}

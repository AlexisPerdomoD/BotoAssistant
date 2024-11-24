using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace Boto.Models;

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
    public DateTime LastLogin { get; }
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
    public Task<(Exception? e, IUsr? usr)> CreateUsr(IUsr usr);
    public IUsr? GetCurrentUsr();
    public bool SetCurrentUsr(IUsr usr);
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

}
/// <summary>
///   Interface for a input/output manager that can be used by the main program
/// </summary>
/// <remarks>
///   An input/output manager is able to get user input and manage the history of the input
/// </remarks>
public interface IInputOutputMannager
{
    public string? GetInput(string prompt, Func<string?, bool>? validator = null, string? customTryAgainMessage = null);
    public void ClearHistory();
    public string? LastInput { get; }
    public List<string> History { get; }
}

/// --- Services Models ---

/// <summary>
///   Interface for a service that can be executed by the main program
/// </summary>
/// <remarks>
///   A service is a class that implements this interface and has a Name and a Description
///   It can also have Options, which are classes that implement IServiceOption
/// </remarks>
public interface IServiceOption
{
    public string Name { get; }
    public string Description { get; }
    public bool CleanConsoleRequired { get; }
    public Task<string?> exec(string[]? args = null);
}

public interface IIOMannagerService : IBotoLogger, IInputOutputMannager;
/// <summary>
///   Interface for a service that can be executed by the main program
/// </summary>
/// <remarks>
///   A service is a class that implements this interface and has a Name, a Description and Options to be run and returning a string which is the next name of the follow option or null if finish the service. 
/// </remarks>
public interface IService
{   /// <summary>
    ///   Method to start the service
    /// </summary>
    /// <param name="args">arguments to be passed to the service</param>
    /// <returns>next Option name, else null and finish the service</returns>
    public Task<string?> Start(bool? requiredStartAgain = false);
    /// <summary>
    ///   Input/Output Manager, used to handle the prompts prints on console or files and manage the history of the input. Also includes a ILogger implementation
    /// </summary>
    public IIOMannagerService IOM { get; }
    /// <summary>
    ///   Name of the service, used to identify the service as unique
    /// </summary>
    public string Name { get; }
    /// <summary>
    ///   Description of the service, used to explain what the service does
    /// </summary>
    public string Description { get; }
    /// <summary>
    ///   Dictionary of options for the service, where the key is the option name and the value is the IServiceOption implementation
    /// </summary>
    /// <remarks>
    ///   Every option is indexed by the option name. Make sure this name is unique for every option.
    /// </remarks>
    public ImmutableDictionary<string, IServiceOption> Options { get; }
    /// <summary>
    ///   Method to run the service
    /// </summary>
    /// <returns>next Service name, else null and finish the service</returns>
    public Task<string?> Run();
}

public interface IMainService : IService
{
    /// <summary>
    /// this methos is used to exit the program
    /// </summary>
    public void Goodbye();
}

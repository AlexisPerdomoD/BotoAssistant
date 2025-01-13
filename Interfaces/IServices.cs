using System.Collections.Immutable;
using Boto.Utils;

/// TODO: CREATE interface for Result<T> class
/// TODO: improve interfaces docs
/// TODO: Simplify the interfaces
namespace Boto.interfaces;

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
    string Name { get; }
    string Description { get; }
    bool CleanConsoleRequired { get; }
    Task<Result<string?>> Exec(string[]? args = null);
}

public interface IIOMannagerService : IBotoLogger, IInputOutputMannager;

/// <summary>
///   Interface for a service that can be executed by the main program
/// </summary>
/// <remarks>
///   A service is a class that implements this interface and has a Name, a Description and Options to be run and returning a string which is the next name of the follow option or null if finish the service.
/// </remarks>
public interface IService
{
    /// <summary>
    ///   Method to start the service
    /// </summary>
    /// <param name="args">arguments to be passed to the service</param>
    /// <returns>next Option name, else null and finish the service</returns>
    Task<Result<string?>> Start(bool requiredStartAgain = false);

    /// <summary>
    ///   Input/Output Manager, used to handle the prompts prints on console or files and manage the history of the input. Also includes a ILogger implementation
    /// </summary>
    protected IIOMannagerService IOM { get; }

    /// <summary>
    ///   Name of the service, used to identify the service as unique
    /// </summary>
    string Name { get; }

    /// <summary>
    ///   Description of the service, used to explain what the service does
    /// </summary>
    string Description { get; }

    /// <summary>
    ///   Dictionary of options for the service, where the key is the option name and the value is the IServiceOption implementation
    /// </summary>
    /// <remarks>
    ///   Every option is indexed by the option name. Make sure this name is unique for every option.
    /// </remarks>
    ImmutableDictionary<string, IServiceOption> Options { get; }

    /// <summary>
    ///   Method to run the service
    /// </summary>
    /// <returns>next Service name, else null and finish the service</returns>
    Task<Result<string?>> Run();
}

/// <summary>
///   Interface for a service that is executed as the main service, so far it contains the ability to exit the program.
/// </summary>
/// <remarks>
/// </remarks>
public interface IMainService
{
    /// <summary>
    /// this methos is used to exit the program
    /// </summary>
    void GoodBye();
}

/// <summary>
///   Interface for a service that can make used of Users management
/// </summary>
public interface IUserService
{
    IUsrMannager Mngr { get; }
}

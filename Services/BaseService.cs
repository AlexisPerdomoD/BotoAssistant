using System.Collections.Immutable;
using Boto.interfaces;
using Boto.Setup;
using Boto.Utils;

namespace Boto.Services;

/// <summary>
/// Base service class with optional abstractions for default implementations.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><term><b>Name</b></term><description>Name of the service.</description></item>
/// <item><term><b>Description</b></term><description>Description of the service.</description></item>
/// <item><term><b>Options</b></term><description>Dictionary of options for the service, where the key is the option name and the value is the <c>IServiceOption</c> implementation.</description></item>
/// <item><term><b>Start</b></term><description>Method to start the service.</description></item>
/// <item><term><b>Run</b></term><description>Method to run the service.</description></item>
/// </list>
/// </remarks>
public abstract class BaseService(IIOMannagerService iom, string name, string description)
    : IService
{
    public IIOMannagerService IOM => iom;
    public string Name => name;
    public string Description => description;
    public abstract ImmutableDictionary<string, IServiceOption> Options { get; }
    public abstract Task<Result<string?>> Start(bool requiredStartAgain = false);
    public static string WorkDir => Env.WorkingDirectory;
    protected static string FmtOptsList(ImmutableDictionary<string, IServiceOption> options)
    {
        string formatted = $"Options List:\n";
        foreach (var (key, value) in options)
        {
            formatted += $"Service: {key.PadRight(10)} -> {value.Description}\n";
        }
        formatted += $"\n{"Type 'exit' to exit".PadRight(30)}\n";
        return formatted;
    }

    public async Task<Result<string?>> Run()
    {
        IOM.ClearLogs();
        var clearFinalLogs = true;
        var startProcess = await this.Start();
        if (!startProcess.IsOk)
            return startProcess;
        var input = startProcess.Value;
        if (string.IsNullOrWhiteSpace(input))
        {
            IOM.LogInformation($"No input provided. Exiting Service {Name}.\n");
            return null;
        }
        while (true)
        {
            if (input == "service child done")
            {
                input = IOM.GetInput(FmtOptsList(Options));
                continue;
            }
            if (string.IsNullOrWhiteSpace(input))
            {
                IOM.LogInformation($"No input provided. Exiting Service {Name}.\n");
                break;
            }
            if (input == "exit")
            {
                IOM.LogInformation($"Exiting Service {Name}.\n");
                break;
            }

            if (!Options.TryGetValue(input, out IServiceOption? option))
            {
                IOM.ClearLogs();
                input = IOM.GetInput(
                    $"Option {input} not found.\nPlease enter a valid option name.\n\n{FmtOptsList(Options)}"
                );
                continue;
            }
            if (option.CleanConsoleRequired)
                IOM.ClearLogs();
            var res = await option.Exec();
            input = res.Match(
                i => i,
                err =>
                {
                    clearFinalLogs = false;
                    IOM.ClearLogs();
                    IOM.LogWarning($"Error: {err.Type}\n${err.Message}\n\nPlease try again.");
                    return "exit";
                }
            );
        }
        if (input == "exit")
        {
            if (clearFinalLogs)
                IOM.ClearLogs();
            IOM.LogInformation($"Service {Name} exited.\n");
            input = "service child done";
        }
        return input;
    }
}
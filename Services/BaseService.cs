using System.Collections.Immutable;
using Boto.Models;

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
    public abstract Task<string?> Start(bool requiredStartAgain = false);

    protected static string FmtOptsList(ImmutableDictionary<string, IServiceOption> options)
    {
        string formatted = $"Options:\n";
        foreach (var (key, value) in options)
        {
            formatted += $"Service:{key.PadRight(30)} -> {value.Description}\n";
        }
        formatted += $"\n{"Type 'exit' to exit".PadRight(30)}";
        return formatted;
    }

    public async Task<string?> Run()
    {
        Console.Clear();
        string? input = await this.Start();
        bool requiredStartAgain = false;
        if (string.IsNullOrWhiteSpace(input))
            return null;
        while (input != "exit")
        {
            if (requiredStartAgain)
            {
                input = await this.Start(requiredStartAgain);
                requiredStartAgain = false;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                input = this.IOM.GetInput(
                    "Please enter a valid option name.\nElse service will be ended.\n"
                );
                if (string.IsNullOrWhiteSpace(input))
                    break;
            }

            if (!this.Options.TryGetValue(input, out IServiceOption? option))
            {
                Console.Clear();
                this.IOM.LogInformation($"Option {input} not found.\n");
                requiredStartAgain = true;
                continue;
            }

            if (option.CleanConsoleRequired)
                Console.Clear();
            input = await option.Exec();
        }

        this.IOM.LogInformation($"Exiting Service {this.Name}.\n");
        return input;
    }
}

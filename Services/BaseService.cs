using Boto.Models;
using System.Collections.Immutable;
namespace Boto.Services;
/// <summary>
/// Base Service class with optional abstractions for default implementations.
/// </summary>
public abstract class BaseService(IIOMannagerService iom, string name, string description, ImmutableDictionary<string, IServiceOption> options) : IService
{
    public IIOMannagerService IOM => iom;
    public string Name => name;
    public string Description => description;
    public ImmutableDictionary<string, IServiceOption> Options => options;
    public abstract Task<string?> Start(bool? requiredStartAgain = false);

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
        while (input != "exit")
        {
            if (requiredStartAgain)
            {
                input = await this.Start(requiredStartAgain);
                requiredStartAgain = false;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                input = this.IOM.GetInput("Please enter a valid option name.\nElse service will be ended.\n");
                if (string.IsNullOrWhiteSpace(input)) break;
            }

            if (!this.Options.TryGetValue(input, out IServiceOption? option))
            {
                Console.Clear();
                this.IOM.LogInformation($"Option {input} not found.\n");
                requiredStartAgain = true;
                continue;
            }

            if (option.CleanConsoleRequired) Console.Clear();
            input = await option.exec();
        };
        this.IOM.LogInformation($"Exiting Service {this.Name}.\n");
        return input;
    }
}

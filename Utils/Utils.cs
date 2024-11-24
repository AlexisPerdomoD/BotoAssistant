using Boto.Models;
namespace Boto.Utils;


public abstract class ServiceUtils<T>(T intance) where T : ServiceUtils<T>, IService
{
    private readonly T _intance = intance;

    public async Task<string?> Run()
    {
        Console.Clear();
        string? input = await this._intance.Start();
        bool requiredStartAgain = false;
        while (input != "exit")
        {
            if (requiredStartAgain)
            {
                input = await this._intance.Start(requiredStartAgain);
                requiredStartAgain = false;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                input = this._intance.IOM.GetInput("Please enter a valid option name.\nElse service will be ended.\n");
                if (string.IsNullOrWhiteSpace(input)) break;
            }

            if (!this._intance.Options.TryGetValue(input, out IServiceOption? option))
            {
                Console.Clear();
                this._intance.IOM.LogInformation($"Option {input} not found.\n");
                continue;
            }

            if (option.CleanConsoleRequired) Console.Clear();
            input = await option.exec();


        };

        this._intance.IOM.LogInformation($"Exiting Service {this._intance.Name}.\n");
        return input;
    }

}


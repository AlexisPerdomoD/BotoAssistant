using Boto.Models;

namespace Boto.Services.ServiceOption;

public class ServiceOpt(
    string name,
    string description,
    bool cleanConsoleRequired,
    Func<string[]?, Task<string?>> exec
) : IServiceOption
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public bool CleanConsoleRequired { get; } = cleanConsoleRequired;

    public Task<string?> Exec(string[]? args = null) => exec(args);
}

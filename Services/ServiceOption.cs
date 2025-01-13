using Boto.interfaces;
using Boto.Utils;

namespace Boto.Services;

public class ServiceOpt(
    string name,
    string description,
    bool cleanConsoleRequired,
    Func<string[]?, Task<Result<string?>>> exec
) : IServiceOption
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public bool CleanConsoleRequired { get; } = cleanConsoleRequired;

    public Task<Result<string?>> Exec(string[]? args = null) => exec(args);
}

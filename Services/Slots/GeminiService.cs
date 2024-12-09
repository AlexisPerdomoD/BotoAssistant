using System.Collections.Immutable;
using Boto.Models;

/// --- Services Models ---
/// <summary>
///   Slots reference services for the main service
/// </summary
namespace Boto.Services.Slots;

public class GeminiService(IIOMannagerService iom)
    : BaseService(
        iom,
        name: "Gemini",
        description: "Gemini IA assistant Service, this module includes calling the API, list old conversations, etc",
        options: _options
    )
{
    private static readonly ImmutableDictionary<string, IServiceOption> _options = new Dictionary<
        string,
        IServiceOption
    >
    {
        {
            "chat",
            new ServiceOption(
                name: "chat",
                description: "Start a chat with the AI assistant.",
                cleanConsoleRequired: false,
                exec: static (args) =>
                {
                    if (args.Length == 0) { }
                    return Task.FromResult<string?>(null);
                }
            )
        }
    }.ToImmutableDictionary();

    public override Task<string?> Start(bool requiredStartAgain = false)
    {
        string prompt;
        if (!requiredStartAgain)
        {
            prompt = this.IOM.GetInput("");
        }
        return Task.FromResult<string?>(null);
    }
}

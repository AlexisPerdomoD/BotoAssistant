using System.Collections.Immutable;
using Boto.Models;
using Boto.Services.ServiceOption;

namespace Boto.Services.Gemini;

public class GeminiService(IIOMannagerService iom, IUsrMngr usrMngr)
    : BaseService(
        iom,
        name: "Gemini",
        description: "Gemini IA assistant Service, this module includes calling the API, list old conversations, etc"
    )
{
    private Chat? _currentChat { get; set; }

    public override ImmutableDictionary<string, IServiceOption> Options =>
        new Dictionary<string, IServiceOption>
        {
            {
                "new chat",
                new ServiceOpt(
                    name: "new chat",
                    description: "Start a chat with the AI assistant.",
                    cleanConsoleRequired: true,
                    exec: (_args) =>
                    {
                        var usr = usrMngr.GetCurrentUsr();
                        if (usr == null)
                        {
                            this.IOM.LogInformation("No user is currently set. Finishing chat.\n");
                            return Task.FromResult<string?>(null);
                        }
                        string? i = IOM.GetInput(
                            $"Hello {usr.Name}! what do you want to chat about?\n"
                        );
                        if (string.IsNullOrWhiteSpace(i))
                        {
                            this.IOM.LogInformation("Finishing chat.\n");
                            return Task.FromResult<string?>(null);
                        }
                        var chat = new Chat();
                        chat.Add(Chat.Rol.User, i);
                        _currentChat = chat;
                        var body = chat.ToJson();
                        // TODO: call the API
                        // TODO: Print the response
                        // TODO: ask if the user wants to continue ??
                        return Task.FromResult<string?>(null);
                    }
                )
            }
        }.ToImmutableDictionary(); // TODO: pass this Dictionary as an intance outside of the class to avoid creating a ImmutableDictionary intance everytime

    // TODO: Call the API METHOD
    // TODO: LIST OLD CONVERSATIONS METHOD
    // TODO: DELETE OLD CONVERSATIONS METHOD
    // TODO: SAVE CONVERSATIONS on FILE SYSTEM METHOD
    // TODO: PRINT ON TERM OR EDITOR A OLD CONVERSATION METHOD

    public override Task<string?> Start(bool requiredStartAgain = false)
    {
        if (!requiredStartAgain)
        {
            this.IOM.LogInformation("Starting Gemini Service...\n");
        }
        string? opt = this.IOM.GetInput(FmtOptsList(this.Options));

        return Task.FromResult(opt);
    }
}

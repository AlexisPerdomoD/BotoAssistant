using System.Collections.Immutable;
using System.Text;
using Boto.Models;
using Boto.Services.ServiceOption;

namespace Boto.Services.Gemini;

public class GeminiService(IIOMannagerService iom, IUsrMannager usrMngr)
    : BaseService(
        iom,
        name: "Gemini",
        description: "Gemini IA assistant Service, this module includes calling the API, list old conversations, etc"
    )
{
    private readonly IUsrMannager _usrMngr = usrMngr;
    private readonly HttpClient _apiClient = new();
    private Chat? _currentChat { get; set; }

    // TODO: Call the API METHOD
    public async Task<Chat?> GenerateText(Chat chat, bool stream = true)
    {
        try
        {
            var baseUrl = "https://api.gemini.com/v1/ai/text/chatcompletion";
            var chatType = stream ? "streamGenerateContent?alt=sse&key=" : "generateContent?key=";
            var url = $"{baseUrl}/model/{chat.Model}:{chatType}";
            var content = new StringContent(chat.ToJson(), Encoding.UTF8, "application/json");
            var response = await _apiClient.PostAsync(url, content);
            return null;
        }
        catch (Exception e)
        {
            return null;
        }
    }

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
                        var usr = _usrMngr.GetCurrentUsr();
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
                        chat.Add(Chat.Role.User, i);
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

    // TODO: LIST OLD CONVERSATIONS METHOD
    // TODO: DELETE OLD CONVERSATIONS METHOD
    // TODO: SAVE CONVERSATIONS on FILE SYSTEM METHOD
    // TODO: PRINT ON TERM OR EDITOR A OLD CONVERSATION METHOD

    public override Task<string?> Start(bool requiredStartAgain = false)
    {
        if (!requiredStartAgain)
        {
            IOM.LogInformation(
                $"Gemini Assistant to help you with your AI needs {_usrMngr.GetCurrentUsr()?.Name ?? ""}!\n"
            );
        }
        string? opt = IOM.GetInput(FmtOptsList(this.Options));

        return Task.FromResult(opt);
    }
}

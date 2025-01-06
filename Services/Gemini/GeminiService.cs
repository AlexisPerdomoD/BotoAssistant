using System.Collections.Immutable;
using System.Text;
using Boto.Models;
using Boto.Services.ServiceOption;
using Boto.Setup;
using Boto.Utils;

namespace Boto.Services.Gemini;

public class GeminiService(IIOMannagerService iom, IUsrMannager usrMngr)
    : BaseService(
        iom,
        name: "Gemini",
        description: "Gemini IA assistant Service, this module includes calling the API, list old conversations, etc"
    )
{
    private static readonly HttpClient _apiClient = new();
    private readonly IUsrMannager _usrMngr = usrMngr;

    private Chat? _chat { get; set; }

    // TODO: Call the API METHOD
    public async Task<bool> GenerateText(string text, bool stream = true)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;
        _chat ??= new Chat();
        _chat.Add(Chat.Role.User, text);

        var baseUrl = "https://generativelanguage.googleapis.com/v1beta";
        var chatType = stream ? "streamGenerateContent?alt=sse&key=" : "generateContent?key=";
        var url = $"{baseUrl}/models/{_chat.Model}:{chatType}{Env.GeminiApiKey}";
        var content = new StringContent(_chat.ToJson(), Encoding.UTF8, "application/json");
        var response = await _apiClient.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            IOM.LogWarning(
                $"Error calling the API: {response.StatusCode} --> {response.ReasonPhrase}\n"
            );
            IOM.LogInformation("sorry about that, try again later\n");
            return false;
        }

        if (!stream)
        {
            return true;
        }

        using var streamReader = new StreamReader(response.Content.ReadAsStream());
        while (!streamReader.EndOfStream)
        {
            var line = await streamReader.ReadLineAsync();
            if (line == null)
                continue;
            //_chat.Add(Chat.Role.Assistant, line);
            IOM.LogInformation(line + "\n\n");
        }

        return true;
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
                    exec: async (_args) =>
                    {
                        var usr = _usrMngr.GetCurrentUsr();
                        if (usr == null)
                        {
                            this.IOM.LogInformation("No user is currently set. Finishing chat.\n");
                            return null;
                        }
                        string? i = IOM.GetInput(
                            $"Hello {usr.Name}! what do you want to chat about?\n"
                        );
                        if (string.IsNullOrWhiteSpace(i))
                        {
                            this.IOM.LogInformation("Finishing chat.\n");
                            return null;
                        }
                        var res = await GenerateText(i);
                        // TODO: call the API
                        // TODO: Print the response
                        // TODO: ask if the user wants to continue ??
                        return null;
                    }
                )
            },
        }.ToImmutableDictionary(); // TODO: pass this Dictionary as an intance outside of the class to avoid creating a ImmutableDictionary intance everytime

    // TODO: LIST OLD CONVERSATIONS METHOD
    // TODO: DELETE OLD CONVERSATIONS METHOD
    // TODO: SAVE CONVERSATIONS on FILE SYSTEM METHOD
    // TODO: PRINT ON TERM OR EDITOR A OLD CONVERSATION METHOD

    public override Task<Result<string?>> Start(bool requiredStartAgain = false)
    {
        if (!requiredStartAgain)
        {
            IOM.LogInformation(
                $"Gemini Assistant to help you with your AI needs {_usrMngr.GetCurrentUsr()?.Name ?? ""}!\n"
            );
        }
        string? opt = IOM.GetInput(FmtOptsList(this.Options));

        return Task.FromResult(Result<string?>.Ok(opt));
    }
}

using System.Collections.Immutable;
using System.Text;
using Boto.interfaces;
using Boto.Setup;
using Boto.Utils;
using Boto.Utils.Json;

namespace Boto.Services.Gemini;

public class GeminiService(IIOMannagerService iom, Func<IUsr?> GetUsr)
    : BaseService(
        iom,
        name: "Gemini",
        description: "Gemini IA assistant Service, this module includes calling the API, list old conversations, etc"
    )
{
    private static readonly HttpClient _apiClient = new();
    public virtual string BaseUrl { get; } = "https://generativelanguage.googleapis.com/v1beta";
    private IUsr _usr => GetUsr() ?? throw new InvalidCastException("Not User provided");

    private ChatG? _chat { get; set; }

    private async Task<Result<bool>> _chatting(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;
        _chat ??= new ChatG();
        _chat.Add(ChatG.Role.User, text);
        var baseUrl = BaseUrl;
        var chatType = "streamGenerateContent?alt=sse&key=";
        var url = $"{baseUrl}/models/{_chat.Model}:{chatType}{Env.GeminiApiKey}";
        var content = new StringContent(_chat.ToJson(), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        var requestType = HttpCompletionOption.ResponseHeadersRead; // start reading as soon as the headers are set
        var response = await _apiClient.SendAsync(request, requestType);

        if (!response.IsSuccessStatusCode)
        {
            var message =
                $"Error calling the API: {response.StatusCode} --> {response.ReasonPhrase}\n";
            return Err.ExternalError(message);
        }

        var readingTask = ChatGRes.Read(await response.Content.ReadAsStreamAsync(), true);
        var result = await Result<(ChatGRes, string)>.FromTask(readingTask);

        if (!result.IsOk)
            return result.Err;

        var (_, resMesage) = result.Value;
        _chat.Add(ChatG.Role.Model, resMesage);
        return true;
    }

    private async Task<Result<bool>> _saveChat()
    {
        if (_chat is null)
            return Err.InvalidInput("No Current chat is active");
        var baseDir = $"{WorkDir}/chats";
        if (!Directory.Exists(baseDir))
        {
            _ = Directory.CreateDirectory(baseDir);
        }

        var content = _chat.ToJson();
        var filepath = $"{baseDir}/{_usr.Name}-{DateTime.Now}";
        var writtingTask = File.WriteAllTextAsync(filepath, content);
        var result = await Result<None>.FromTask(writtingTask);
        return (!result.IsOk) ? result.Err : true;
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
                        string? i = IOM.GetInput(
                            $"Hello {_usr.Name}! what do you want to chat about?\n"
                        );
                        while (true)
                        {
                            if (string.IsNullOrWhiteSpace(i))
                            {
                                this.IOM.LogInformation("Not input provided, finishing chat.\n");
                                return "child process done";
                            }
                            var res = await _chatting(i);
                            if (!res.IsOk)
                            {
                                IOM.LogWarning(res.Err.Message);
                                IOM.LogInformation("sorry about that, try again later\n");

                                return "child process done";
                            }
                            i = IOM.GetInput("\n\nOther question? just press enter if not\n\n");
                            if (string.IsNullOrWhiteSpace(i))
                                break;
                            continue;
                        }
                        var message =
                            "Would you like to save conversation?\n\n- type 'yes' to save\n - Press enter to continue";
                        var save = IOM.GetInput(message);
                        if (message == "yes")
                        {
                            IOM.LogInformation("Saving last chat...");
                            _ = _saveChat();
                        }
                        _chat = null;
                        return "exit";
                    }
                )
            },
        }.ToImmutableDictionary();

    // TODO: LIST OLD CONVERSATIONS METHOD
    // TODO: DELETE OLD CONVERSATIONS METHOD
    // TODO: SAVE CONVERSATIONS on FILE SYSTEM METHOD
    // TODO: PRINT ON TERM OR EDITOR A OLD CONVERSATION METHOD

    public override Task<Result<string?>> Start(bool requiredStartAgain = false)
    {
        if (!requiredStartAgain)
        {
            IOM.LogInformation(
                $"Gemini Assistant to help you with your AI needs {_usr.Name ?? ""}!\n"
            );
        }
        string? opt = IOM.GetInput(FmtOptsList(this.Options));

        return Task.FromResult(Result<string?>.Ok(opt));
    }
}

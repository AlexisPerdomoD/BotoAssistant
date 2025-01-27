using Boto.interfaces;
using Boto.Services;
using Boto.Services.Gemini;
using Boto.Setup;
using Boto.Usr;
using Boto.Utils;
using Microsoft.Extensions.Logging;

var iom = new SpectreConsole(LogLevel.Information);
var usrMngr = new UsrMannager(iom);
var gemini = new GeminiService(iom, usrMngr.GetCurrentUsr);
var MainOptions = new Dictionary<string, IServiceOption>
{
    {
        "gemini",
        new ServiceOpt(
            name: "gemini",
            description: "Gemini IA assistant Service, this module includes calling the API, list old conversations, etc",
            cleanConsoleRequired: true,
            exec: async _ =>
            {
                if (Env.GeminiApiKey is null)
                {
                    var message = "Gemini API key is not set, please set it in the config file";
                    return Err.AccessDenied(message);
                }
                var res = await gemini.Run();
                return res;
            }
        )
    },
};

// IN PROGREES: IMPLEMENT Spectre.Console library
// implement selectable options to IOM interfaces and so
// implement colorfull messages
// implement loading visual when necessary
// https://spectreconsole.net/prompts/text
// or https://www.nuget.org/packages/Terminal.Gui/ as alternative
// TODO: Implement Markdown output for IA responses
// TODO: Implement chat box or something in order to paste multiline text
// TODO: CREATE DOC
MainService mainService = new(iom, "Boto", "Main Service", MainOptions, usrMngr);
await mainService.Run();
mainService.GoodBye();
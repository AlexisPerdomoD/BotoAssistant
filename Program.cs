using System.Collections.Immutable;
using Boto.Models;
using Boto.Services;
using Boto.Services.Gemini;
using Boto.Services.ServiceOption;
using Boto.Utils;
using Microsoft.Extensions.Logging;

var iom = new MainIOMannager(LogLevel.Information);
var usrMngr = new UsrMannager(iom);
var gemini = new GeminiService(iom, usrMngr);
var MainOptions = new Dictionary<string, IServiceOption>
{
    {
        "gemini",
        new ServiceOpt(
            name: "gemini",
            description: "Gemini IA assistant Service, this module includes calling the API, list old conversations, etc",
            cleanConsoleRequired: true,
            exec: async _ => await gemini.Run()
        )
    }
}.ToImmutableDictionary();

MainService mainService = new(iom, "Boto", "Main Service", MainOptions, usrMngr);
await mainService.Run();
mainService.GoodBye();

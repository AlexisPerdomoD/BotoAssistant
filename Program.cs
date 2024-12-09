using System.Collections.Immutable;
using Boto.Models;
using Boto.Services;
using Boto.Utils;
using Microsoft.Extensions.Logging;

MainIOMannager iom = new(LogLevel.Information);
UsrMngr usrMngr = new(iom);
MainService mainService =
    new(iom, "Boto", "Main Service", ImmutableDictionary<string, IServiceOption>.Empty, usrMngr);
await mainService.Run();

mainService.GoodBye();

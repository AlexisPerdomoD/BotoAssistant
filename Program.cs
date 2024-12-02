using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

using Boto.Utils;
using Boto.Services;
using Boto.Models;

MainIOMannager iom = new(LogLevel.Information);
UsrMngr usrMngr = new(iom);
MainService mainService = new(iom, "Boto", "Main Service", ImmutableDictionary<string, IServiceOption>.Empty, usrMngr);
await mainService.Run();
mainService.GoodBye();


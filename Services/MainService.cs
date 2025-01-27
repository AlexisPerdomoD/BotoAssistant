using System.Collections.Immutable;
using Boto.interfaces;
using Boto.Utils;

namespace Boto.Services;

public class MainService : BaseService, IMainService, IUserService
{
    private const string _wellcomeMessage =
        $"Welcome to Boto Assistant!\n\nThis program is meant to be used as a terminal interface to mannage IA requests.\nsome files and notes mannagement in order to be used along with other terminal developer tools.\n\nWho are you?\n\ntype \"exit\" to go out.\n\n";
    private const string _end = "service child done";

    public IUsrMannager Mngr { get; }

    public override ImmutableDictionary<string, IServiceOption> Options { get; }

    private ImmutableDictionary<string, IServiceOption> _addGeneralExtrasOptions(
        ref Dictionary<string, IServiceOption> options
    )
    {
        options.Add(
            "show user information",
            new ServiceOpt(
                name: "show user information",
                description: "show basic stash of the current profile",
                cleanConsoleRequired: true,
                exec: _ =>
                {
                    var usr = Mngr.GetCurrentUsr();
                    if (usr is null)
                    {
                        IOM.LogInformation("There is not setted usr, this is not allowed");
                        return Task.FromResult<Result<string?>>(_end);
                    }
                    IOM.LogInformation(usr.GetSts());
                    IOM.WaitInteraction(clearScreen: true);
                    return Task.FromResult<Result<string?>>(_end);
                }
            )
        );
        return options.ToImmutableDictionary();
    }

    public override Task<Result<string?>> Start(bool requiredStartAgain = false)
    {
        if (!requiredStartAgain)
        {
            var name = IOM.GetInput(_wellcomeMessage);

            if (string.IsNullOrWhiteSpace(name) || name == "exit")
            {
                GoodBye();
                return Task.FromResult<Result<string?>>(null);
            }
            var existRes = Mngr.UsrExists(name);

            if (!existRes.IsOk)
            {
                var err = existRes.Err;
                GoodBye($"\nError while checking if user {name} exists.\nEnding program.", err);
            }
            var usr = existRes.Value;
            IOM.ClearLogs();
            if (usr is null)
            {
                IOM.LogInformation($"User {name} does not exist.\nCreating...\n");
                string usrProfile =
                    IOM.GetInput(
                        $"Please enter a general prompt for {name} profile.\nThe idea is use this when consulting AI APIS.\n"
                    ) ?? "";
                string profileTags =
                    IOM.GetInput(
                        $"Please enter a list of tags for {name} profile separated by space.\nThe idea is use this when consulting AI APIS.\n"
                    ) ?? "";

                var createRes = Mngr.CreateUsr(name, usrProfile, profileTags.Split(" "));
                if (!createRes.IsOk)
                {
                    var err = createRes.Err;
                    GoodBye($"\nError while creating user {name}.\nEnding program.", err);
                }
                IOM.LogInformation($"User {name} created.\n");
            }
            else
            {
                IOM.LogInformation($"User {name} exists.\nWelcome back!\n");
                var usrSettledRes = Mngr.SetCurrentUsr(ref usr);
                if (!usrSettledRes.IsOk)
                {
                    var err = usrSettledRes.Err;
                    GoodBye($"\nError while setting user {name}.\nEnding program.", err);
                }
            }
        }
        var response = IOM.GetInput(FmtOptsList(Options));
        return Task.FromResult<Result<string?>>(response);
    }

    public void GoodBye()
    {
        IOM.LogInformation("Goodbye! Have a nice day!");
        Environment.Exit(0);
    }

    public void GoodBye(string message)
    {
        IOM.LogInformation(message);
        Environment.Exit(0);
    }

    public void GoodBye(string message, Err err)
    {
        var errType = err.Type;
        var errMessage = err.Message;
        IOM.LogWarning($"{errType}: {errMessage}");

        IOM.LogInformation(message);
        Environment.Exit(0);
    }

    public MainService(
        IIOMannagerService iom,
        string name,
        string description,
        Dictionary<string, IServiceOption> options,
        IUsrMannager usrMngr
    )
        : base(iom, name, description)
    {
        Mngr = usrMngr;
        Options = _addGeneralExtrasOptions(ref options);
    }
}
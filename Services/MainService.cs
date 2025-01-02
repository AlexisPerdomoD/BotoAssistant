using System.Collections.Immutable;
using Boto.Models;

namespace Boto.Services;

public class MainService(
    IIOMannagerService iom,
    string name,
    string description,
    ImmutableDictionary<string, IServiceOption> options,
    IUsrMannager usrMngr
) : BaseService(iom, name, description), IMainService, IUserService
{
    public IUsrMannager Mngr { get; } = usrMngr;
    public override ImmutableDictionary<string, IServiceOption> Options { get; } = options;

    public override async Task<string?> Start(bool requiredStartAgain = false)
    {
        if (!requiredStartAgain)
        {
            string? name = this.IOM.GetInput(
                $"Welcome to Boto Assistant!\n\nThis program is meant to be used as a terminal interface to mannage IA requests.\nsome files and notes mannagement in order to be used along with other terminal developer tools.\n\nWho are you?\n\ntype \"exit\" to go out.\n\n"
            );

            if (string.IsNullOrWhiteSpace(name) || name == "exit")
            {
                this.GoodBye();
                return null;
            }
            var (e, usr) = await this.Mngr.UsrExists(name);
            if (e != null)
                GoodBye($"\nError while checking if user {name} exists.\nEnding program.");

            IOM.ClearLogs();
            if (usr == null)
            {
                this.IOM.LogInformation($"User {name} does not exist.\nCreating...\n");
                string usrProfile =
                    this.IOM.GetInput(
                        $"Please enter a general prompt for {name} profile.\nThe idea is use this when consulting AI APIS.\n"
                    ) ?? "";
                string profileTags =
                    IOM.GetInput(
                        $"Please enter a list of tags for {name} profile separated by space.\nThe idea is use this when consulting AI APIS.\n"
                    ) ?? "";
                (e, _) = await this.Mngr.CreateUsr(name, usrProfile, profileTags.Split(" "));
                if (e != null)
                    GoodBye($"\nError while creating user {name}.\nEnding program.");
                IOM.LogInformation($"User {name} created.\n");
            }
            else
            {
                IOM.LogInformation($"User {name} exists.\nWelcome back!\n");
                var usrSettled = await this.Mngr.SetCurrentUsr(usr);
                if (!usrSettled)
                    GoodBye($"\nError while setting user {name} as current user.\nEnding program.");
            }
        }
        return this.IOM.GetInput(FmtOptsList(Options));
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
}

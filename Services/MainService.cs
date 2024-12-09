using System.Collections.Immutable;
using Boto.Models;

namespace Boto.Services;

public class MainService(
    IIOMannagerService iom,
    string name,
    string description,
    ImmutableDictionary<string, IServiceOption> options,
    IUsrMngr usrMngr
) : BaseService(iom, name, description, options), IMainService, IUserService
{
    public IUsrMngr Mngr { get; } = usrMngr;

    public override async Task<string?> Start(bool requiredStartAgain = false)
    {
        try
        {
            if (!requiredStartAgain)
            {
                string? name = this.IOM.GetInput(
                    $"Welcome to Boto Assistant!\n\nThis program is meant to be used as a terminal interface to mannage IA requests.\nsome files and notes mannagement in order to be used along with other terminal developer tools.\n\nWho are you?\n\ntype \"exit\" to go out."
                );

                if (string.IsNullOrWhiteSpace(name) || name == "exit")
                {
                    this.GoodBye();
                    return null;
                }
                var (e, usr) = await this.Mngr.UsrExists(name);
                if (e != null)
                {
                    this.IOM.LogError($"Error while checking if user {name} exists.", e);
                    return null;
                }

                if (usr == null)
                {
                    this.IOM.LogInformation($"User {name} does not exist.Creating...\n");
                    string usrProfile =
                        this.IOM.GetInput(
                            $"Please enter a general prompt for {name} profile.\nThe idea is use this when consulting AI APIS.\n"
                        ) ?? "";
                    string profileTags =
                        this.IOM.GetInput(
                            $"Please enter a list of tags for {name} profile separated by space.\nThe idea is use this when consulting AI APIS.\n"
                        ) ?? "";
                    (e, usr) = await this.Mngr.CreateUsr(name, usrProfile, profileTags.Split(" "));
                    if (e != null)
                    {
                        this.IOM.LogError($"Error while creating user {name}.", e);
                        return null;
                    }
                    this.IOM.LogInformation($"User {name} created.\n");
                }
                else
                {
                    this.IOM.LogInformation($"User {name} exists.\nWelcome back!\n");
                    bool usrSettled = await this.Mngr.SetCurrentUsr(usr);
                    if (!usrSettled)
                    {
                        this.IOM.LogError(
                            $"Error while setting user {name} as current user.",
                            new InvalidDataException(
                                "Error happend while trying to set current user property"
                            )
                        );
                        return null;
                    }
                }
            }
            string optionList = FmtOptsList(this.Options);
            return this.IOM.GetInput($"{optionList}\n");
        }
        catch (Exception e)
        {
            this.IOM.LogError($"Error while starting the main service.", e);
            return null;
        }
    }

    public void GoodBye()
    {
        this.IOM.LogInformation("Goodbye! Have a nice day!");
        Environment.Exit(0);
    }
}

using System.Text.Json;
using Boto.Setup;

namespace Boto.Models;

public class UsrMngr(IBotoLogger logger) : IUsrMngr
{
    public static string Wdir => Env.WorkingDirectory;
    public static string AppMode => Env.AppMode;
    private readonly IBotoLogger _logger = logger;
    private IUsr? _currentUsr;

    public async Task<(Exception? e, IUsr? usr)> UsrExists(string usrName)
    {
        try
        {
            string usrPath = Path.Combine(Wdir, $"usr/{usrName}.json");
            if (!File.Exists(usrPath))
                return (null, null);

            string usrFileText = await File.ReadAllTextAsync(usrPath);
            IUsr usr =
                JsonSerializer.Deserialize<Usr>(usrFileText)
                ?? throw new JsonException("Failed to deserialize usr file or it is empty");

            return (null, usr);
        }
        catch (Exception e)
        {
            this._logger.LogInformation(
                "Error happen while verifying user.\nProgram will be finished.",
                true
            );

            if (AppMode == "DEV")
                this._logger.LogError(e.Message, e);
            return (e, null);
        }
    }

    public async Task<(Exception? e, IUsr? usr)> CreateUsr(
        string usrName,
        string usrProfile,
        string[] profileTags
    )
    {
        try
        {
            if (!Directory.Exists($"{Wdir}/usr"))
                _ = Directory.CreateDirectory($"{Wdir}/usr");

            string usrPath = Path.Combine(Wdir, $"usr/{usrName}.json");
            IUsr usr = new Usr(usrName, usrProfile, profileTags);
            string usrFileText = JsonSerializer.Serialize(usr);
            await File.WriteAllTextAsync(usrPath, usrFileText);
            this._currentUsr = usr;
            return (null, usr);
        }
        catch (Exception e)
        {
            return (e, null);
        }
    }

    public async Task<bool> SetCurrentUsr(IUsr? usr)
    {
        if (usr == null)
        {
            this._currentUsr = null;
            return true;
        }
        var (e, user) = await this.UsrExists(usr.Name);
        if (e != null)
        {
            this._logger.LogError($"Error while checking if user {usr.Name} exists.", e);
            return false;
        }
        if (user == null)
        {
            this._logger.LogInformation($"User {usr.Name} does not exist.");
            return false;
        }

        this._currentUsr = usr;
        return true;
    }

    public IUsr? GetCurrentUsr() => this._currentUsr;
}

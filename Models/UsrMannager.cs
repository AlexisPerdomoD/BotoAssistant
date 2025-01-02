using System.Text.Json;
using Boto.Setup;
using Boto.Utils.Json;

namespace Boto.Models;

public class UsrMannager(IBotoLogger logger) : IUsrMannager
{
    public static readonly string Wdir = Env.WorkingDirectory;
    public static readonly string AppMode = Env.AppMode;
    private readonly IBotoLogger _errorLogger = logger;
    private IUsr? _currentUsr;

    public async Task<(Exception? e, IUsr? usr)> UsrExists(string usrName)
    {
        try
        {
            usrName = usrName.ToLowerInvariant().Trim();
            string usrPath = Path.Combine(Wdir, $"usr/{usrName}.json");
            if (!File.Exists(usrPath))
                return (null, null);
            string usrFileText = await File.ReadAllTextAsync(usrPath);
            IUsr? usr = JsonSerializer.Deserialize(usrFileText, UsrJsonContext.Default.Usr);
            return (null, usr);
        }
        catch (Exception e)
        {
            _errorLogger.LogInformation(
                "Error happen while verifying user.\nProgram will be finished.",
                true
            );

            if (AppMode == "DEV")
                _errorLogger.LogError(e.Message, e);
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
            string usrFileText = JsonSerializer.Serialize(usr, UsrJsonContext.Default.Usr);
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
        if (usr is null)
        {
            this._currentUsr = null;
            return true;
        }
        var (e, user) = await this.UsrExists(usr.Name);
        if (e != null)
        {
            _errorLogger.LogError($"Error while checking if user {usr.Name} exists.", e);
            return false;
        }
        if (user == null)
        {
            _errorLogger.LogInformation($"User {usr.Name} does not exist.");
            return false;
        }
        usr.LastLogin = DateTime.Now;
        _ = await usr.SaveUsrSts(); // TODO: Check if return string Error
        this._currentUsr = usr;
        return true;
    }

    public async Task<bool> SetCurrentUsr(string usrName)
    {
        var (e, usr) = await this.UsrExists(usrName);
        if (e != null)
        {
            _errorLogger.LogError($"Error while checking if user {usrName} exists.", e);
            return false;
        }
        if (usr == null)
        {
            _errorLogger.LogInformation($"User {usrName} does not exist.");
            return false;
        }

        usr.LastLogin = DateTime.Now;
        _ = await usr.SaveUsrSts(); // TODO: Check if return string Error
        this._currentUsr = usr;
        return true;
    }

    public IUsr? GetCurrentUsr() => this._currentUsr;
}
using System.Text.Json;
using Boto.interfaces;
using Boto.Setup;
using Boto.Utils;
using Boto.Utils.Json;

namespace Boto.Usr;

public class UsrMannager(IBotoLogger logger) : IUsrMannager
{
    public static readonly string Wdir = Env.WorkingDirectory;
    public static readonly string AppMode = Env.AppMode;
    private readonly IBotoLogger _errorLogger = logger;
    private IUsr? _currentUsr;

    public Result<IUsr?> UsrExists(string usrName)
    {
        try
        {
            usrName = usrName.ToLowerInvariant().Trim();
            if (string.IsNullOrWhiteSpace(usrName))
            {
                return Err.InvalidInput("usrName not valid");
            }

            string usrPath = Path.Combine(Wdir, $"usr/{usrName}.json");
            if (!File.Exists(usrPath))
                return null;

            var usrFileText = File.ReadAllText(usrPath);
            var context = BotoJsonSerializerContext.Default.Usr;
            Usr? usr = JsonSerializer.Deserialize(usrFileText, context);
            return usr;
        }
        catch (Exception e)
        {
            if (AppMode == "DEV")
                _errorLogger.LogError(e.Message, e);

            var err = e switch
            {
                JsonException => Err.InvalidInput(e.Message),
                FileLoadException => Err.ProgramError(e.Message),
                _ => Err.UnknownError(e.Message),
            };
            return err;
        }
    }

    public Result<IUsr> CreateUsr(string usrName, string usrProfile, string[] profileTags)
    {
        try
        {
            if (!Directory.Exists($"{Wdir}/usr"))
            {
                _ = Directory.CreateDirectory($"{Wdir}/usr");
            }

            string usrPath = Path.Combine(Wdir, $"usr/{usrName}.json");
            Usr usr = new Usr(usrName, usrProfile, profileTags);
            var context = BotoJsonSerializerContext.Default.Usr;
            var usrFileText = JsonSerializer.Serialize(usr, context);
            File.WriteAllText(usrPath, usrFileText);
            this._currentUsr = usr;
            return usr;
        }
        catch (Exception e)
        {
            var err = e switch
            {
                JsonException => Err.InvalidInput(e.Message),
                FileLoadException => Err.ProgramError(e.Message),
                _ => Err.UnknownError(e.Message),
            };
            return err;
        }
    }

    public Result<bool> SetCurrentUsr(ref IUsr usr)
    {
        var res = UsrExists(usr.Name);
        if (!res.IsOk)
            return res.Err;
        if (res.Value is null)
            return Err.InvalidInput("Usr does not exist on db");

        usr.LastLogin = DateTime.Now;
        _currentUsr = usr;
        _ = usr.SaveUsrSts(); // TODO: Check if return string Error
        return true;
    }

    public Result<bool> SetCurrentUsr(ref string usrname)
    {
        var res = UsrExists(usrname);
        if (!res.IsOk)
            return res.Err;
        var usr = res.Value;
        if (usr == null)
            return Err.InvalidInput("Usr does not exist on db");

        usr.LastLogin = DateTime.Now;
        _ = usr.SaveUsrSts(); // TODO: Check if return string Error
        _currentUsr = usr;
        return true;
    }

    public IUsr? GetCurrentUsr() => _currentUsr;

    public void RemoveCurrentUsr() => _currentUsr = null;
}

using Boto.Setup;
using System.Text.Json;

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
            string usrPath = Path.Combine(Wdir, $"/usr/{usrName}.json");
            if (!File.Exists(usrPath)) return (null, null);
            string usrFileText = await File.ReadAllTextAsync(usrPath);
            IUsr usr = JsonSerializer.Deserialize<IUsr>(usrFileText) ?? throw new JsonException("Failed to deserialize usr file or it is empty");
            return (null, usr);
        }
        catch (Exception e)
        {
            this._logger.LogInformation("Error happen while verifying user.\nProgram will be finished.", true);

            if (AppMode == "DEV") this._logger.LogError(e.Message, e);
            return (e, null);
        }
    }


    public async Task<(Exception? e, IUsr? usr)> CreateUsr(IUsr usr)
    {
        try
        {
            if (!Directory.Exists($"{Wdir}/usr")) _ = Directory.CreateDirectory($"{Wdir}/usr");

            string usrPath = Path.Combine(Wdir, $"/usr/{usr.Name}.json");
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
    public bool SetCurrentUsr(IUsr usr)
    {
        // TODO: check if user exists and other validaations
        this._currentUsr = usr;
        return true;
    }
    public IUsr? GetCurrentUsr() => this._currentUsr;

}

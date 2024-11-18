namespace Boto.Models;
using Boto.Setup;
using Boto.Utils;
using System.Text.Json;
public interface IUsr
{
    public string Name { get; set; }
    public string UsrProfile { get; set; }
    public DateTime lastLogin { get; set; }
    public string[] ProfileTags { get; set; }

    /// <summary>
    ///   Method to load the user's STS data, usually on "${BOTO_WORKING_DIRECTORY}/usr/usr.json"
    /// </summary>
    /// <param name="usr"></param>
    /// <returns>error message if any, else null</returns>
    public Task<string?> SaveUsrSts();

}
public interface IUsrMannager
{
    public static abstract string Wdir { get; }
    public Task<(bool? e, IUsr? usr)> UsrExists(string usr);
}

public class Usr : IUsr
{
    public string Wdir => Env.WORKING_USER_DIRECTORY;
    public string Name { get; set; }
    public string UsrProfile { get; set; }
    public DateTime lastLogin { get; set; }
    public string[] ProfileTags { get; set; }
    private string _path { get; }

    public async Task<string?> SaveUsrSts()
    {
        try
        {
            if (!Directory.Exists($"{Wdir}/usr")) Directory.CreateDirectory($"{Wdir}/usr");
            string usrPath = Path.Combine(Wdir, $"/usr/{Name}.json");
            var usrFileText = JsonSerializer.Serialize<Usr>(this);
            await File.WriteAllTextAsync(usrPath, usrFileText);
            return null;
        }
        catch (Exception e)
        {
            return $"{e.GetType().ToString()}\n{e.Message}\n{e.StackTrace}";

        }
    }

    Usr(string name, string usrProfile, string[] profileTags, string usrPath)
    {
        if (string.IsNullOrEmpty(name.Trim()))
            throw new ArgumentNullException(nameof(name));

        this.Name = name.Trim().ToLower();
        this.UsrProfile = usrProfile;
        this.ProfileTags = profileTags;
        this._path = usrPath;
        this.lastLogin = DateTime.Now;
    }
}

public class UsrMannager : IUsrMannager
{
    public static string Wdir => Env.WORKING_USER_DIRECTORY;
    private IBotoLogger _logger;
    public async Task<(bool? e, IUsr? usr)> UsrExists(string usrName)
    {
        try
        {
            string usrPath = Path.Combine(Wdir, $"/usr/{usrName}.json");
            if (!File.Exists(usrPath)) return (true, null);

            if (!Directory.Exists($"{Wdir}/usr")) Directory.CreateDirectory($"{Wdir}/usr");

            var usrFileText = await File.ReadAllTextAsync(usrPath);
            IUsr usr = JsonSerializer.Deserialize<IUsr>(usrFileText) ?? throw new Exception("Failed to deserialize usr file or it is empty");
            return (null, usr);

        }
        catch (Exception e)
        {
            string errorType = e.GetType().ToString();
            this._logger.LogError(errorType, e);
            return (true, null);
        }

    }

    public UsrMannager(Boto.Utils.IBotoLogger logger)
    {
        _logger = logger;
    }
}

using Boto.Setup;
using Boto.Utils;
using System.Text.Json;

namespace Boto.Models;

public interface IUsr
{
    public string Name { get; set; }
    public string UsrProfile { get; set; }
    public DateTime LastLogin { get; set; }
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
    public Task<(bool? e, IUsr? usr)> UsrExists(string usrName);
}

public class Usr : IUsr
{
    public static string Wdir => Env.WorkingDirectory;
    public string Name { get; set; }
    public string UsrProfile { get; set; }
    public DateTime LastLogin { get; set; }
    public string[] ProfileTags { get; set; }
    private string _path { get; }

    public async Task<string?> SaveUsrSts()
    {
        try
        {
            if (!Directory.Exists($"{Wdir}/usr"))
            {
                DirectoryInfo dir = Directory.CreateDirectory($"{Wdir}/usr");
                Console.WriteLine($"Created directory {dir.FullName}\n");
            }
            string usrPath = Path.Combine(Wdir, $"/usr/{this.Name}.json");
            string usrFileText = JsonSerializer.Serialize(this);
            await File.WriteAllTextAsync(usrPath, usrFileText);
            return null;
        }
        catch (Exception e)
        {
            return $"{e.GetType().Name}\n{e.Message}\n{e.StackTrace}";

        }
    }

    public Usr(string name, string usrProfile, string[] profileTags, string usrPath)
    {
        if (string.IsNullOrEmpty(name.Trim()))
            throw new ArgumentNullException(nameof(name));


        this.Name = name.ToLowerInvariant().Trim();
        this.UsrProfile = usrProfile;
        this.ProfileTags = profileTags;
        this._path = usrPath;
        this.LastLogin = DateTime.Now;
    }
}

public class UsrMannager(IBotoLogger logger) : IUsrMannager


{
    public static string Wdir => Env.WorkingDirectory;
    private readonly IBotoLogger _logger = logger;

    public async Task<(bool? e, IUsr? usr)> UsrExists(string usrName)
    {
        try
        {
            string usrPath = Path.Combine(Wdir, $"/usr/{usrName}.json");
            if (!File.Exists(usrPath)) return (true, null);

            if (!Directory.Exists($"{Wdir}/usr"))
            {
                _ = Directory.CreateDirectory($"{Wdir}/usr");
            }

            string usrFileText = await File.ReadAllTextAsync(usrPath);
            IUsr usr = JsonSerializer.Deserialize<IUsr>(usrFileText) ?? throw new JsonException("Failed to deserialize usr file or it is empty");
            return (null, usr);

        }
        catch (Exception e)
        {
            string errorType = e.GetType().ToString();
            this._logger.LogError(errorType, e);
            return (true, null);
        }

    }


}

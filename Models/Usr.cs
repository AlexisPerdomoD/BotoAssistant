using System.Text.Json;
using Boto.Setup;

namespace Boto.Models;

public class Usr : IUsr
{
    private static readonly string _wdir = Env.WorkingDirectory;
    private readonly string _path;
    public string Name { get; private set; }
    public string UsrProfile { get; set; }
    public string[] ProfileTags { get; set; }
    public DateTime LastLogin { get; set; }

    public async Task<string?> SaveUsrSts()
    {
        try
        {
            if (!Directory.Exists($"{_wdir}/usr"))
            {
                DirectoryInfo dir = Directory.CreateDirectory($"{_wdir}/usr");
                Console.WriteLine($"Created directory {dir.FullName}\n");
            }
            string usrFileText = JsonSerializer.Serialize(this);
            await File.WriteAllTextAsync(_path, usrFileText);
            return null;
        }
        catch (Exception e)
        {
            return $"{e.GetType().Name}\n{e.Message}\n{e.StackTrace}";
        }
    }

    public Usr(string name, string usrProfile, string[] profileTags)
    {
        if (string.IsNullOrEmpty(name.Trim()))
            throw new ArgumentNullException(nameof(name));

        Name = name.ToLowerInvariant().Trim();
        UsrProfile = usrProfile;
        ProfileTags = profileTags;
        LastLogin = DateTime.Now;
        _path = Path.Combine(_wdir, $"/usr/{Name}.json");
    }
}

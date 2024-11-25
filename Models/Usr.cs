using System.Text.Json;

using Boto.Setup;
namespace Boto.Models;

public class Usr : IUsr
{
    private static string _wdir => Env.WorkingDirectory;
    private string _path => Path.Combine(_wdir, $"/usr/{this.Name}.json");
    public string Name { get; private set; }
    public string UsrProfile { get; set; }
    public string[] ProfileTags { get; set; }
    public DateTime LastLogin { get; private set; }

    public async Task<string?> SaveUsrSts()
    {
        try
        {
            if (!Directory.Exists($"{_wdir}/usr"))
            {
                DirectoryInfo dir = Directory.CreateDirectory($"{_wdir}/usr");
                Console.WriteLine($"Created directory {dir.FullName}\n");
            }
            string usrPath = Path.Combine(_wdir, $"/usr/{this.Name}.json");
            string usrFileText = JsonSerializer.Serialize(this);
            await File.WriteAllTextAsync(usrPath, usrFileText);
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

        this.Name = name.ToLowerInvariant().Trim();
        this.UsrProfile = usrProfile;
        this.ProfileTags = profileTags;
        this.LastLogin = DateTime.Now;
    }
}

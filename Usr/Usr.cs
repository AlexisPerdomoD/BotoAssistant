using Boto.interfaces;
using Boto.Setup;
using Boto.Utils.Json;
using static System.IO.Directory;
using static System.Text.Json.JsonSerializer;

namespace Boto.Usr;

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
            if (!Exists($"{_wdir}/usr"))
            {
                var dir = CreateDirectory($"{_wdir}/usr");
                Console.WriteLine($"Created directory {dir.FullName}\n");
            }
            var context = BotoJsonSerializerContext.Default.Usr;
            var json = Serialize(this, context);
            await File.WriteAllTextAsync(_path, json);
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

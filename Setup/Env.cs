namespace Boto.Setup;

public class Env
{
    public static string WORKING_USER_DIRECTORY => Environment.GetEnvironmentVariable("BOTO_WORKING_DIRECTORY") ?? throw new InvalidOperationException("BOTO_WORKING_DIRECTORY is not set");
}

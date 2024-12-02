namespace Boto.Setup;

public class Env
{
    public static string WorkingDirectory => Environment.GetEnvironmentVariable("BOTO_WORKING_DIRECTORY") ?? throw new InvalidOperationException("BOTO_WORKING_DIRECTORY is not set");
    public static string AppMode => Environment.GetEnvironmentVariable("BOTO_APP_MODE") ?? "DEV";
}

namespace Boto.Setup;

public record Env
{
    public static readonly string AppMode =
        Environment.GetEnvironmentVariable("BOTO_APP_MODE") ?? "DEV";

    public static readonly string WorkingDirectory =
        Environment.GetEnvironmentVariable("BOTO_WORKING_DIRECTORY")
        ?? throw new InvalidOperationException("BOTO_WORKING_DIRECTORY is not set");

    public static readonly string? GeminiApiKey = Environment.GetEnvironmentVariable(
        "BOTO_GEMINI_API_KEY"
    );
}

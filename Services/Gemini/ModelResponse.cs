using System.Text.Json.Serialization;

namespace Boto.Services.Gemini;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Response))]
public partial class JsonResponse : JsonSerializerContext { }

public class UsageMetadata
{
    public int promptTokenCount;
    public int candidatesTokenCount;
    public int totalTokenCount;
}

public record Response()
{
    public Chat.Content[]? Candidates;
    public UsageMetadata? usageMetadata;
    public string? modelVersion;
}

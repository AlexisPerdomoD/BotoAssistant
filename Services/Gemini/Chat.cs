using System.Text.Json;
using System.Text.Json.Serialization;

namespace Boto.Services.Gemini;

/// Model for serializing and deserializing the Chat class
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(Chat))]
internal partial class ChatJsonContext : JsonSerializerContext { }

internal class Chat(string? model = null)
{
    public static readonly string[] Models = ["gemini-1.5-flash"];
    public readonly string Model = Models.FirstOrDefault(m => m == model) ?? "gemini-1.5-flash";

    public enum Role
    {
        User,
        Assistant
    };

    public record Part(string text);

    public record Content(Role Role, List<Part> Parts);

    private readonly List<Content> _current = [];

    public void Add(Role role, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Assistant Gemini: text is null or empty when adding to chat"
            );
        }

        _current.Add(new Content(role, new([new Part(text)])));
    }

    public string ToJson() => JsonSerializer.Serialize(_current, ChatJsonContext.Default.Chat);
}

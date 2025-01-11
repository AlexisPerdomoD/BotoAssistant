using System.Text.Json;
using System.Text.Json.Serialization;

namespace Boto.Services.Gemini;

/// Model for serializing and deserializing the Chat class
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(List<Chat.Content>))]
[JsonSerializable(typeof(Chat))]
internal partial class ChatJsonContext : JsonSerializerContext { }

public class Chat(string? model = null)
{
    public static readonly string[] Models = ["gemini-1.5-flash"];
    public string Model { get; init; } =
        Models.FirstOrDefault(m => m == model) ?? "gemini-1.5-flash";

    public enum Role
    {
        User,
        Model,
    };

    public record Part(string text);

    public record Content(string Role, List<Part> Parts);

    private readonly List<Content> _current = [];

    public void Add(Role role, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Assistant Gemini: text is null or empty when adding to chat"
            );
        }
        var roleStr = role switch
        {
            Role.User => "user",
            Role.Model => "model",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null),
        };
        _current.Add(new Content(roleStr, new([new Part(text)])));
    }

    public virtual string ToJson()
    {
        var contents = JsonSerializer.Serialize(_current, ChatJsonContext.Default.ListContent);
        return "{contents: " + contents + "}";
    }
}
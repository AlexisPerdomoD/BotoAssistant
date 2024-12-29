using System.Text.Json;

namespace Boto.Services.Gemini;

internal class Chat
{
    public enum Rol
    {
        User,
        Assistant
    };

    public record Part(string text);

    public record Content(Rol role, List<Part> parts);

    private readonly List<Content> _current = new();

    public void Add(Rol role, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Assistant Gemini: text is null or empty when adding to chat"
            );
        }

        _current.Add(new Content(role, new([new Part(text)])));
    }

    public string ToJson() => JsonSerializer.Serialize(_current);
}

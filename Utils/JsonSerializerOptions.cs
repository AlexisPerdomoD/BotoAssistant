using System.Text.Json.Serialization;

namespace Boto.Utils.Json;

/// Model for serializing and deserializing the Usr class
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(List<Services.Gemini.ChatG.Content>))]
[JsonSerializable(typeof(Services.Gemini.ChatG))]
[JsonSerializable(typeof(Services.Gemini.ChatGRes))]
[JsonSerializable(typeof(Services.Gemini.ChatGRes.ChatGResReader))]
[JsonSerializable(typeof(Usr.Usr))]
public partial class BotoJsonSerializerContext : JsonSerializerContext { }

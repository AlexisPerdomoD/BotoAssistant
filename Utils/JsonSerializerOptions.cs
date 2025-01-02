using System.Text.Json.Serialization;
using Boto.Models;

namespace Boto.Utils.Json;

/// Model for serializing and deserializing the Usr class
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(Usr))]
public partial class UsrJsonContext : JsonSerializerContext { }

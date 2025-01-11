using System.Text.Json;
using System.Text.Json.Serialization;
using Boto.Utils;

namespace Boto.Services.Gemini;

/**
 * {
  "candidates": [
    {
      "content": {
        "parts": [
          {
            "text": ""
          }
        ],
        "role": "model"
      },
      "finishReason": "STOP",
      "avgLogprobs": -0.15774411548768855
    }
  ],
  "usageMetadata": {
    "promptTokenCount": 26,
    "candidatesTokenCount": 692,
    "totalTokenCount": 718
  },
  "modelVersion": "gemini-1.5-flash"
}
*/
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ChatRes))]
public partial class JsonResponse : JsonSerializerContext { }

public record ChatRes
{
    public record Metadata(
        double? PromptTokenCount,
        double? CandidatesTokenCount,
        double? TotalTokenCount
    );

    public record Candidate(Chat.Content Content);

    public Candidate[] Candidates { get; private set; }
    public Metadata UsageMetadata { get; private set; }
    public string? ModelVersion { get; private set; }

    private ChatRes(Candidate candidates, Metadata usageMetadata, string? modelVersion)
    {
        Candidates = [candidates];
        UsageMetadata = usageMetadata;
        ModelVersion = modelVersion;
    }

    private ChatRes(Candidate[] candidates, Metadata usageMetadata, string? modelVersion)
    {
        Candidates = candidates;
        UsageMetadata = usageMetadata;
        ModelVersion = modelVersion;
    }

    public static async Task<Result<ChatRes>> Read(Stream stream)
    {
        try
        {
            var res = await JsonSerializer.DeserializeAsync(stream, JsonResponse.Default.ChatRes);
            if (res is null)
            {
                var message = "Chat Response was not provided on the readed stream";
                return Result<ChatRes>.Failure(Err.UnknownError(message));
            }
            return res;
        }
        catch (Exception e)
        {
            var err = e switch
            {
                HttpRequestException => Err.NetworkError(e.Message),
                JsonException => Err.InvalidInput(e.Message),
                _ => Err.ProgramError(e.Message),
            };

            return Result<ChatRes>.Failure(err);
        }
    }
}

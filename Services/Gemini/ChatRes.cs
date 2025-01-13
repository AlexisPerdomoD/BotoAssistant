using System.Text;
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
[JsonSerializable(typeof(ChatRes.ChatResReader))]
public partial class JsonResponse : JsonSerializerContext { }

public record ChatRes
{
    public record Candidate(Chat.Content Content);

    public record Metadata(
        double? PromptTokenCount,
        double? CandidatesTokenCount,
        double? TotalTokenCount
    );

    public struct ChatResReader
    {
        public Candidate[]? Candidates { get; set; }
        public Metadata? UsageMetadata { get; set; }
        public string? ModelVersion { get; set; }

        public override readonly string ToString() =>
            JsonSerializer.Serialize(this, JsonResponse.Default.ChatResReader);
    }

    public Candidate[] Candidates { get; set; }
    public Metadata? UsageMetadata { get; set; }
    public string? ModelVersion { get; set; }

    public override string ToString() =>
        JsonSerializer.Serialize(this, JsonResponse.Default.ChatRes);

    public ChatRes(Candidate data, Metadata? metadata, string? specificModel)
    {
        Candidates = [data];
        UsageMetadata = metadata;
        ModelVersion = specificModel;
    }

    private static ChatResReader _processPartialJson(string pjson)
    {
        if (pjson.StartsWith("data: ", StringComparison.InvariantCulture))
            pjson = pjson["data: ".Length..];
        var jsonType = JsonResponse.Default.ChatResReader;
        var partialChat = JsonSerializer.Deserialize(pjson, jsonType);
        return partialChat;
    }

    public static async Task<Result<(ChatRes res, string message)>> Read(
        Stream stream,
        bool verbose
    )
    {
        try
        {
            var reader = new StreamReader(stream);
            var messageBuilder = new StringBuilder();

            string? resSpecificModel = null;
            double? promptTokenCount = null;
            double? candidatesTokenCount = null;
            double? totalTokenCount = null;

            while (!reader.EndOfStream)
            {
                var partialJson = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(partialJson))
                    continue;
                var partialChat = _processPartialJson(partialJson);
                var meta = partialChat.UsageMetadata;
                var candidates = partialChat.Candidates;
                var model = partialChat.ModelVersion;

                if (meta is not null)
                {
                    promptTokenCount ??= meta.PromptTokenCount;
                    candidatesTokenCount ??= meta.CandidatesTokenCount;
                    totalTokenCount ??= meta.TotalTokenCount;
                }

                if (candidates is not null && candidates.Length > 0)
                {
                    foreach (var c in candidates)
                    {
                        if (c?.Content?.Parts is null || c.Content.Parts.Count == 0)
                            continue;
                        var tempMessage = string.Join(" ", c.Content.Parts[0].text);

                        if (verbose)
                            Console.Write(tempMessage);
                        _ = messageBuilder.Append(tempMessage);
                    }
                }
                if (model is not null)
                    resSpecificModel = model;
            }
            var message = messageBuilder.ToString();
            var candidate = new Candidate(new("model", new([new(message)])));
            var metadata = new Metadata(promptTokenCount, candidatesTokenCount, totalTokenCount);
            var res = new ChatRes(candidate, metadata, resSpecificModel);

            return (res, message);
        }
        catch (Exception e)
        {
            var err = e switch
            {
                HttpRequestException => Err.NetworkError(e.Message),
                JsonException => Err.InvalidInput(e.Message),
                _ => Err.ProgramError(e.Message),
            };

            return Result<(ChatRes, string)>.Failure(err);
        }
    }
}

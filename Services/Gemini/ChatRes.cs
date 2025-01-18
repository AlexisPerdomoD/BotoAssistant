using System.Text;
using System.Text.Json;
using Boto.Utils.Json;

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
public record ChatGRes
{
    public record Candidate(ChatG.Content Content);

    public record Metadata(
        double? PromptTokenCount,
        double? CandidatesTokenCount,
        double? TotalTokenCount
    );

    public struct ChatGResReader
    {
        public Candidate[]? Candidates { get; set; }
        public Metadata? UsageMetadata { get; set; }
        public string? ModelVersion { get; set; }

        public override readonly string ToString() =>
            JsonSerializer.Serialize(this, BotoJsonSerializerContext.Default.ChatGResReader);
    }

    public Candidate[] Candidates { get; set; }
    public Metadata? UsageMetadata { get; set; }
    public string? ModelVersion { get; set; }

    public override string ToString() =>
        JsonSerializer.Serialize(this, BotoJsonSerializerContext.Default.ChatGRes);

    public ChatGRes(Candidate data, Metadata? metadata, string? specificModel)
    {
        Candidates = [data];
        UsageMetadata = metadata;
        ModelVersion = specificModel;
    }

    private static ChatGResReader _processPartialJson(string pjson)
    {
        if (pjson.StartsWith("data: ", StringComparison.InvariantCulture))
            pjson = pjson["data: ".Length..];
        var jsonType = BotoJsonSerializerContext.Default.ChatGResReader;
        var partialChat = JsonSerializer.Deserialize(pjson, jsonType);
        return partialChat;
    }

    public static async Task<(ChatGRes res, string message)> Read(Stream stream, bool verbose)
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
        var res = new ChatGRes(candidate, metadata, resSpecificModel);

        return (res, message);
    }
}
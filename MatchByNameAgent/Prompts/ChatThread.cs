using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI.Chat;

namespace Qonq.Reasoning.Agents.Prompts;

public class ChatThread
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() }
    };

    public List<ChatMessage> Messages { get; } = new();

    public void AddSystemMessage(string content)
        => Messages.Add(new SystemChatMessage(content));

    public void AddUserMessage(string content)
        => Messages.Add(new UserChatMessage(content));

    public void AddDataSample<T>(T data)
        => Messages.Add(new UserChatMessage(JsonSerializer.Serialize(data, JsonOptions)));

    public void GenerateJsonOutput(params SampleObject[] samples)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Respond with a JSON object only. No markdown. No explanation.");
        sb.AppendLine("Examples:");

        foreach (var sample in samples)
        {
            sb.AppendLine($"Example {sample.Description}:");
            sb.AppendLine(JsonSerializer.Serialize(sample.Data, JsonOptions));
        }

        Messages.Add(new UserChatMessage(sb.ToString()));
    }
}

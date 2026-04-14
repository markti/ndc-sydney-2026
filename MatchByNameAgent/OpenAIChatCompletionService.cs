using System.Text;
using Qonq.Reasoning.Agents.Interfaces;
using Qonq.Reasoning.Agents.Models;
using OpenAI.Chat;

namespace Qonq.Reasoning.Agents;

public class OpenAIChatCompletionService : IChatCompletionService
{
    private readonly ChatClient _chatClient;

    public OpenAIChatCompletionService(ChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<ChatCompletionResult> CompleteAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken = default)
    {
        var response = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        var completion = response.Value;

        var sb = new StringBuilder();
        foreach (var content in completion.Content)
        {
            sb.AppendLine(content.Text);
        }

        return new ChatCompletionResult(sb.ToString().Trim(), completion.Usage?.TotalTokenCount ?? 0);
    }
}

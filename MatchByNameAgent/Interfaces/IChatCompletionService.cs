using Qonq.Reasoning.Agents.Models;
using OpenAI.Chat;

namespace Qonq.Reasoning.Agents.Interfaces;

public interface IChatCompletionService
{
    Task<ChatCompletionResult> CompleteAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken = default);
}

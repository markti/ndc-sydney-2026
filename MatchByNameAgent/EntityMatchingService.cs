using System.Text.Json;
using System.Text.Json.Serialization;
using Qonq.Reasoning.Agents.Interfaces;
using Qonq.Reasoning.Agents.Models;
using Qonq.Reasoning.Agents.Prompts;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.Text;

namespace Qonq.Reasoning.Agents;

public class EntityMatchingService : IEntityMatchingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ILogger<EntityMatchingService> _logger;
    private readonly ChatClient _chatClient;

    public EntityMatchingService(
        ILogger<EntityMatchingService> logger,
        ChatClient chatCompletionService)
    {
        _logger = logger;
        _chatClient = chatCompletionService;
    }

    public async Task<MatchResult> MatchAsync(
        MatchCandidate matchCandidate,
        IEnumerable<IMatchableEntity> existingEntities,
        CancellationToken stoppingToken)
    {
        var thread = new ChatThread();
        thread.AddSystemMessage("You are an Entity Matching Assistant. Your job is to decide whether an incoming entity (with a name and one or more aliases) matches any existing entity in a provided list, or if it does not exist in the current set.");
        thread.AddUserMessage("Here is the current set of entities:");
        thread.AddDataSample(existingEntities);
        thread.AddUserMessage("Here is the incoming entity name and aliases: ");
        thread.AddDataSample(matchCandidate);
        thread.AddUserMessage("Use the property of 'id' to get the matching entity's ID if one exists");
        // NDC DEMO
        thread.AddUserMessage("Sometimes weird people use numbers to replace letters");
        thread.AddUserMessage("Sometimes people use stock tickers as synonyms");

        var sample1 = new SampleObject()
        {
            Description = "with Match Type of 'ExactMatch'",
            Data = new EntityMatchReasoning()
            {
                MatchType = EntityMatchType.ExactMatch,
                MatchingEntityId = Guid.NewGuid().ToString(),
                Explanation = "It's an exact match",
                Confidence = 1
            }
        };
        var sample2 = new SampleObject()
        {
            Description = "with Match Type of 'ProbableMatch'",
            Data = new EntityMatchReasoning()
            {
                MatchType = EntityMatchType.ProbableMatch,
                MatchingEntityId = Guid.NewGuid().ToString(),
                Explanation = "It's pretty close, but I have my doubts",
                Confidence = 0.75m
            }
        };
        var sample3 = new SampleObject()
        {
            Description = "with Match Type of 'NoMatch'",
            Data = new EntityMatchReasoning()
            {
                MatchType = EntityMatchType.NoMatch,
                MatchingEntityId = Guid.NewGuid().ToString(),
                Explanation = "There is no match",
                Confidence = 0.93m
            }
        };
        thread.GenerateJsonOutput(sample1, sample2, sample3);

        var result = await _chatClient.CompleteChatAsync(thread.Messages);
        var completionResult = result.Value;

        var tokenUsage = completionResult.Usage;
        _logger.LogInformation("TOKENS input={Input}, output={Output}, total={Total}", tokenUsage?.InputTokenCount ?? 0, tokenUsage?.OutputTokenCount ?? 0, tokenUsage?.TotalTokenCount ?? 0);

        var responseBuilder = new StringBuilder();

        foreach (var content in completionResult.Content)
        {
            responseBuilder.AppendLine(content.Text);
        }

        _logger.LogInformation($"Entity Matching Service: CHAT RESPONSE: {responseBuilder.ToString()}");

        var responseText = responseBuilder.ToString();

        var responseOutput = JsonSerializer.Deserialize<EntityMatchReasoning>(responseText, JsonOptions);

        if (responseOutput == null || responseOutput.MatchType == EntityMatchType.NoMatch)
        {
            return new MatchResult(null, responseOutput ?? new EntityMatchReasoning { MatchType = EntityMatchType.NoMatch });
        }

        var match = existingEntities.FirstOrDefault(p => p.UniqueId == responseOutput.MatchingEntityId);
        if (match == null)
        {
            return new MatchResult(null, responseOutput);
        }

        return new MatchResult(MergeAliases(match, matchCandidate), responseOutput);
    }

    private static IMatchableEntity MergeAliases(IMatchableEntity match, MatchCandidate candidate)
    {
        var matchedEntity = new MatchedEntity()
        {
            UniqueId = match.UniqueId,
            Name = match.Name
        };

        var aliasSet = match.Aliases != null
            ? new HashSet<string>(
                match.Aliases.Where(a => !string.IsNullOrWhiteSpace(a)),
                StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(candidate.Name) &&
            !string.Equals(match.Name, candidate.Name, StringComparison.OrdinalIgnoreCase))
        {
            aliasSet.Add(candidate.Name);
        }

        if (candidate.Aliases != null)
        {
            foreach (var alias in candidate.Aliases.Where(a => !string.IsNullOrWhiteSpace(a)))
            {
                aliasSet.Add(alias);
            }
        }

        matchedEntity.Aliases = aliasSet.ToList();

        return matchedEntity;
    }
}

using Qonq.Reasoning.Agents;
using Qonq.Reasoning.Agents.Interfaces;
using Qonq.Reasoning.Agents.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OpenAI.Chat;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Qonq.Reasoning.Agents.UnitTests;

public class EntityMatchingServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IChatCompletionService _chatCompletionService = Substitute.For<IChatCompletionService>();
    private readonly EntityMatchingService _sut;

    public EntityMatchingServiceTests()
    {
        _sut = new EntityMatchingService(
            NullLogger<EntityMatchingService>.Instance,
            _chatCompletionService);
    }

    private static List<IMatchableEntity> BuildEntityList() =>
    [
        new MatchedEntity { UniqueId = "entity-1", Name = "Acme Corporation", Aliases = ["Acme Corp", "Acme"] },
        new MatchedEntity { UniqueId = "entity-2", Name = "Globex Corporation", Aliases = null }
    ];

    private void SetupAiResponse(EntityMatchReasoning reasoning)
    {
        var json = JsonSerializer.Serialize(reasoning, JsonOptions);
        _chatCompletionService
            .CompleteAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<CancellationToken>())
            .Returns(new ChatCompletionResult(json, 0));
    }

    [Fact]
    public async Task MatchAsync_WhenAiReturnsNoMatch_ReturnsNull()
    {
        SetupAiResponse(new EntityMatchReasoning
        {
            MatchType = EntityMatchType.NoMatch,
            Explanation = "Nothing close enough",
            Confidence = 0.95m
        });

        var result = await _sut.MatchAsync(
            new MatchCandidate { Name = "Unknown Inc" },
            BuildEntityList(),
            CancellationToken.None);

        Assert.Null(result.Entity);
    }

    [Fact]
    public async Task MatchAsync_WhenAiReturnsExactMatch_ReturnsMatchedEntity()
    {
        SetupAiResponse(new EntityMatchReasoning
        {
            MatchType = EntityMatchType.ExactMatch,
            MatchingEntityId = "entity-1",
            Explanation = "Identical name",
            Confidence = 1m
        });

        var result = await _sut.MatchAsync(
            new MatchCandidate { Name = "Acme Corporation" },
            BuildEntityList(),
            CancellationToken.None);

        Assert.NotNull(result.Entity);
        Assert.Equal("entity-1", result.Entity.UniqueId);
        Assert.Equal("Acme Corporation", result.Entity.Name);
    }

    [Fact]
    public async Task MatchAsync_WhenAiReturnsProbableMatch_ReturnsMatchedEntity()
    {
        SetupAiResponse(new EntityMatchReasoning
        {
            MatchType = EntityMatchType.ProbableMatch,
            MatchingEntityId = "entity-2",
            Explanation = "Very similar name",
            Confidence = 0.85m
        });

        var result = await _sut.MatchAsync(
            new MatchCandidate { Name = "Globex Corp" },
            BuildEntityList(),
            CancellationToken.None);

        Assert.NotNull(result.Entity);
        Assert.Equal("entity-2", result.Entity.UniqueId);
        Assert.Equal("Globex Corporation", result.Entity.Name);
    }

    [Fact]
    public async Task MatchAsync_WhenAiReturnsUnknownEntityId_ReturnsNull()
    {
        SetupAiResponse(new EntityMatchReasoning
        {
            MatchType = EntityMatchType.ExactMatch,
            MatchingEntityId = "entity-does-not-exist",
            Explanation = "Hallucinated ID",
            Confidence = 1m
        });

        var result = await _sut.MatchAsync(
            new MatchCandidate { Name = "Acme" },
            BuildEntityList(),
            CancellationToken.None);

        Assert.Null(result.Entity);
    }

    [Fact]
    public async Task MatchAsync_WhenCandidateNameDiffersFromMatch_AddsCandidateNameAsAlias()
    {
        SetupAiResponse(new EntityMatchReasoning
        {
            MatchType = EntityMatchType.ExactMatch,
            MatchingEntityId = "entity-1",
            Explanation = "Alias match",
            Confidence = 0.9m
        });

        var result = await _sut.MatchAsync(
            new MatchCandidate { Name = "ACME" },
            BuildEntityList(),
            CancellationToken.None);

        Assert.NotNull(result.Entity);
        Assert.Contains("ACME", result.Entity.Aliases!, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MatchAsync_MergesCandidateAliasesIntoResult()
    {
        SetupAiResponse(new EntityMatchReasoning
        {
            MatchType = EntityMatchType.ExactMatch,
            MatchingEntityId = "entity-2",
            Explanation = "Match found",
            Confidence = 1m
        });

        var result = await _sut.MatchAsync(
            new MatchCandidate { Name = "Globex", Aliases = ["Global Exchange", "GX Corp"] },
            BuildEntityList(),
            CancellationToken.None);

        Assert.NotNull(result.Entity);
        Assert.Contains("Global Exchange", result.Entity.Aliases!);
        Assert.Contains("GX Corp", result.Entity.Aliases!);
    }

    [Fact]
    public async Task MatchAsync_DoesNotDuplicateExistingAliases()
    {
        SetupAiResponse(new EntityMatchReasoning
        {
            MatchType = EntityMatchType.ExactMatch,
            MatchingEntityId = "entity-1",
            Explanation = "Match found",
            Confidence = 1m
        });

        var result = await _sut.MatchAsync(
            new MatchCandidate { Name = "Acme Corp", Aliases = ["acme corp"] },
            BuildEntityList(),
            CancellationToken.None);

        Assert.NotNull(result.Entity);
        var lowerAliases = result.Entity.Aliases!.Select(a => a.ToLowerInvariant()).ToList();
        Assert.Single(lowerAliases, a => a == "acme corp");
    }
}

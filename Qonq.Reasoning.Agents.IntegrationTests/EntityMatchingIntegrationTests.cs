using Qonq.Reasoning.Agents.IntegrationTests.Fixtures;
using Qonq.Reasoning.Agents.Interfaces;
using Qonq.Reasoning.Agents.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace Qonq.Reasoning.Agents.IntegrationTests;

public class EntityMatchingIntegrationTests : IClassFixture<OpenAIFixture>
{
    private readonly EntityMatchingService _sut;
    private readonly ITestOutputHelper _output;

    public EntityMatchingIntegrationTests(OpenAIFixture fixture, ITestOutputHelper output)
    {
        _output = output;

        var chatCompletionService = new OpenAIChatCompletionService(fixture.ChatClient);

        _sut = new EntityMatchingService(
            NullLogger<EntityMatchingService>.Instance,
            chatCompletionService);
    }

    private const string MicrosoftId = "a1b2c3d4-0001-4000-8000-000000000001";
    private const string AppleId     = "a1b2c3d4-0002-4000-8000-000000000002";
    private const string GoogleId    = "a1b2c3d4-0003-4000-8000-000000000003";

    private static List<IMatchableEntity> BuildEntityList() =>
    [
        new MatchedEntity { UniqueId = MicrosoftId, Name = "Microsoft Corporation", Aliases = ["Microsoft", "MSFT"] },
        new MatchedEntity { UniqueId = AppleId,     Name = "Apple Inc",             Aliases = ["Apple"] },
        new MatchedEntity { UniqueId = GoogleId,    Name = "Google LLC",            Aliases = ["Google", "Alphabet"] }
    ];

    private void WriteReasoning(MatchResult result)
    {
        _output.WriteLine($"Match Type:  {result.Reasoning.MatchType}");
        _output.WriteLine($"Confidence:  {result.Reasoning.Confidence:P0}");
        _output.WriteLine($"Reasoning:   {result.Reasoning.Explanation}");
    }

    [Fact]
    public async Task MatchAsync_WithKnownAlias_ReturnsMatchedEntity()
    {
        var candidate = new MatchCandidate { Name = "Microsoft" };

        _output.WriteLine($"Candidate: {candidate.Name}");

        var result = await _sut.MatchAsync(candidate, BuildEntityList(), CancellationToken.None);

        WriteReasoning(result);

        if (result.Entity is null)
        {
            _output.WriteLine("Result: no match found");
        }
        else
        {
            _output.WriteLine($"Match found: {result.Entity.Name} (id={result.Entity.UniqueId})");
            if (result.Entity.Aliases?.Count > 0)
                _output.WriteLine($"Aliases: {string.Join(", ", result.Entity.Aliases)}");
        }

        Assert.NotNull(result.Entity);
        Assert.Equal(MicrosoftId, result.Entity.UniqueId);
        Assert.Equal("Microsoft Corporation", result.Entity.Name);
    }

    [Fact]
    public async Task MatchAsync_WithUnknownEntity_ReturnsNull()
    {
        var candidate = new MatchCandidate { Name = "Amazon Web Services" };

        _output.WriteLine($"Candidate: {candidate.Name}");

        var result = await _sut.MatchAsync(candidate, BuildEntityList(), CancellationToken.None);

        WriteReasoning(result);
        _output.WriteLine(result.Entity is null ? "Result: no match found" : $"Match found: {result.Entity.Name} (id={result.Entity.UniqueId})");

        Assert.Null(result.Entity);
    }

    [Fact]
    public async Task MatchAsync_WithSimilarButDistinctName_ReturnsNull()
    {
        // "Micro Technologies" sounds vaguely like "Microsoft" but is a different company
        var candidate = new MatchCandidate { Name = "Micro Technologies Inc" };

        _output.WriteLine($"Candidate: {candidate.Name}");

        var result = await _sut.MatchAsync(candidate, BuildEntityList(), CancellationToken.None);

        WriteReasoning(result);
        _output.WriteLine(result.Entity is null ? "Result: no match found" : $"Match found: {result.Entity.Name} (id={result.Entity.UniqueId})");

        Assert.Null(result.Entity);
    }

    [Fact]
    public async Task MatchAsync_WithClearlyNewEntity_ReturnsNull()
    {
        // "SpaceX" shares nothing with any entity in the list
        var candidate = new MatchCandidate { Name = "SpaceX", Aliases = ["Space Exploration Technologies"] };

        _output.WriteLine($"Candidate: {candidate.Name}");

        var result = await _sut.MatchAsync(candidate, BuildEntityList(), CancellationToken.None);

        WriteReasoning(result);
        _output.WriteLine(result.Entity is null ? "Result: no match found" : $"Match found: {result.Entity.Name} (id={result.Entity.UniqueId})");

        Assert.Null(result.Entity);
    }
}

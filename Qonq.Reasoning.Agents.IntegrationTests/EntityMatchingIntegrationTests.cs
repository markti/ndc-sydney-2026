using Qonq.Reasoning.Agents.IntegrationTests.Fixtures;
using Qonq.Reasoning.Agents.Interfaces;
using Qonq.Reasoning.Agents.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace Qonq.Reasoning.Agents.IntegrationTests;

public class EntityMatchingIntegrationTests : IClassFixture<OpenAIFixture>
{
    private readonly EntityMatchingService _sut;

    public EntityMatchingIntegrationTests(OpenAIFixture fixture)
    {
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

    [Fact]
    public async Task MatchAsync_WithKnownAlias_ReturnsMatchedEntity()
    {
        var candidate = new MatchCandidate { Name = "Microsoft" };

        var result = await _sut.MatchAsync(candidate, BuildEntityList(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(MicrosoftId, result.UniqueId);
        Assert.Equal("Microsoft Corporation", result.Name);
    }

    [Fact]
    public async Task MatchAsync_WithUnknownEntity_ReturnsNull()
    {
        var candidate = new MatchCandidate { Name = "Amazon Web Services" };

        var result = await _sut.MatchAsync(candidate, BuildEntityList(), CancellationToken.None);

        Assert.Null(result);
    }
}

using Qonq.Reasoning.Agents.Models;

namespace Qonq.Reasoning.Agents.Interfaces;

public interface IEntityMatchingService
{
    Task<IMatchableEntity?> MatchAsync(
        MatchCandidate matchCandidate,
        IEnumerable<IMatchableEntity> existingEntities,
        CancellationToken stoppingToken);
}

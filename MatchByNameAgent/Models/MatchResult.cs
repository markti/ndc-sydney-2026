using Qonq.Reasoning.Agents.Interfaces;

namespace Qonq.Reasoning.Agents.Models;

public record MatchResult(IMatchableEntity? Entity, EntityMatchReasoning Reasoning);

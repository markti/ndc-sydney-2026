using Qonq.Reasoning.Agents.Interfaces;

namespace Qonq.Reasoning.Agents.Models;

public class MatchedEntity : IMatchableEntity
{
    public string UniqueId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string>? Aliases { get; set; }
}

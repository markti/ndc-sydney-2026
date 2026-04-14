namespace Qonq.Reasoning.Agents.Interfaces;

public interface IMatchableEntity
{
    string UniqueId { get; set; }
    string Name { get; set; }
    List<string>? Aliases { get; set; }
}

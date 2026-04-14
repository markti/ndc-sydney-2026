using System.Text.Json.Serialization;

namespace Qonq.Reasoning.Agents.Models;

public class EntityMatchReasoning
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EntityMatchType MatchType { get; set; }

    public string? MatchingEntityId { get; set; }
    public string? Explanation { get; set; }
    public decimal Confidence { get; set; }
}

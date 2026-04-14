namespace Qonq.Reasoning.Agents.IntegrationTests.Configuration;

public class OpenAISettings
{
    public const string SectionName = "AOAI";

    public string AccessKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Deployment { get; set; } = string.Empty;
}

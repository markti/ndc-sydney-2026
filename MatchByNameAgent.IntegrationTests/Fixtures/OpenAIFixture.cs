using Azure;
using Azure.AI.OpenAI;
using Qonq.Reasoning.Agents.IntegrationTests.Configuration;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Qonq.Reasoning.Agents.IntegrationTests.Fixtures;

public class OpenAIFixture : IDisposable
{
    public AzureOpenAIClient Client { get; }
    public ChatClient ChatClient { get; }
    public OpenAISettings Settings { get; }

    public OpenAIFixture()
    {
        var testAssembly = FixtureHelpers.GetTestAssembly();

        // Build configuration: environment variables override user-secrets
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(testAssembly, optional: true)
            .Build();

        Settings = new OpenAISettings();
        config.GetSection(OpenAISettings.SectionName).Bind(Settings);

        var apiKeyCredential = new AzureKeyCredential(Settings.AccessKey);

        Client = new AzureOpenAIClient(new Uri(Settings.Endpoint), apiKeyCredential);
        ChatClient = Client.GetChatClient(Settings.Deployment);
    }

    public void Dispose()
    {
        // nothing to dispose; here for pattern completeness
    }
}

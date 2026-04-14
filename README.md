# NDC Sydney 2026 — Match By Name Agent

Demonstrates AI-powered entity matching using Azure OpenAI and the .NET OpenAI SDK.

## Projects

| Project | Purpose |
|---|---|
| `MatchByNameAgent` | Core library — `EntityMatchingService` and supporting types |
| `MatchByNameAgent.UnitTests` | Unit tests using NSubstitute (no network required) |
| `MatchByNameAgent.IntegrationTests` | Integration tests that call a live Azure OpenAI deployment |

---

## Running the Unit Tests

No configuration needed.

```powershell
dotnet test MatchByNameAgent.UnitTests/MatchByNameAgent.UnitTests.csproj
```

---

## Running the Integration Tests

### Prerequisites

- .NET 9 SDK
- Access to an Azure OpenAI resource with a `gpt-4o` deployment

### 1. Configure secrets

Open `MatchByNameAgent.IntegrationTests/setup-secrets.ps1` and set your `AOAI:AccessKey`, then run:

```powershell
.\MatchByNameAgent.IntegrationTests\setup-secrets.ps1
```

This registers the following values in the [.NET user secrets store](https://learn.microsoft.com/aspnet/core/security/app-secrets):

| Key | Description |
|---|---|
| `AOAI:Endpoint` | Azure OpenAI resource endpoint URL |
| `AOAI:AccessKey` | Azure OpenAI API key |
| `AOAI:Deployment` | Model deployment name (e.g. `gpt-4o`) |

Secrets are stored on your machine and are never committed to source control.

### 2. Run the tests

```powershell
dotnet test MatchByNameAgent.IntegrationTests/MatchByNameAgent.IntegrationTests.csproj
```

### CI / environment variables

In a CI pipeline, secrets can be supplied as environment variables instead of user secrets — the configuration loader checks environment variables first:

```
AOAI__Endpoint=https://...
AOAI__AccessKey=...
AOAI__Deployment=gpt-4o
```

> Note: environment variable names use double underscores (`__`) as the section separator on most platforms.

$project = Join-Path $PSScriptRoot "Qonq.Reasoning.Agents.IntegrationTests.csproj"

dotnet user-secrets set "AOAI:Endpoint"   "https://oai-ro-contract-engine-processor-dev-westus3.openai.azure.com/" --project $project
dotnet user-secrets set "AOAI:AccessKey"  ""  --project $project
dotnet user-secrets set "AOAI:Deployment" "gpt-4o"                                                                    --project $project

Write-Host "Secrets configured."

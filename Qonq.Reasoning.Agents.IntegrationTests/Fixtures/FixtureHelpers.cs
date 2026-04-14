using System.Reflection;

namespace Qonq.Reasoning.Agents.IntegrationTests.Fixtures;

public static class FixtureHelpers
{
    public static Assembly GetTestAssembly() => typeof(FixtureHelpers).Assembly;
}

namespace RobsSpacePatrol.CLI.API.Library.Tests;

internal static class ApiTestSettings
{
    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("ROBSSPACEPATROL_API_TEST_URL")
        ?? "https://localhost:5000";

    public static string UserEmail =>
        GetRequiredEnvironmentVariable("ROBSSPACEPATROL_API_TEST_USER");

    public static string AdminEmail =>
        GetRequiredEnvironmentVariable("ROBSSPACEPATROL_API_TEST_ADMIN");

    public static string Password =>
        GetRequiredEnvironmentVariable("ROBSSPACEPATROL_API_TEST_PASSWORD");

    private static string GetRequiredEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException(
                $"Set the {name} environment variable before running live API tests.");
    }
}

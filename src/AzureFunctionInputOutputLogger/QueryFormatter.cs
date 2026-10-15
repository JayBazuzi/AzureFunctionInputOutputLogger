namespace Bazuzi.AzureFunctionInputOutputLogger;

/// <summary>
/// Formats a query string for logging, replacing the values of secret parameters with <c>[redacted]</c>.
/// The key is kept so readers can see the parameter was sent.
/// </summary>
internal static class QueryFormatter
{
    public const string Redacted = "[redacted]";

    // Clients send the Azure Functions key as ?code=..., so it is always redacted.
    private static readonly HashSet<string> RedactedKeys = new(StringComparer.OrdinalIgnoreCase) { "code" };

    public static string Format(IEnumerable<(string Key, string? Value)> parameters)
        => string.Join(
            "&",
            parameters.Select(parameter => $"{parameter.Key}={(RedactedKeys.Contains(parameter.Key) ? Redacted : parameter.Value)}"));
}

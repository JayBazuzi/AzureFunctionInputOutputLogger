namespace Bazuzi.AzureFunctionInputOutputLogger;

/// <summary>
/// Formats a query string for logging, replacing the values of secret parameters with <c>[redacted]</c>.
/// The key is kept so readers can see the parameter was sent.
/// </summary>
internal sealed class QueryFormatter
{
    public const string Redacted = "[redacted]";

    private readonly HashSet<string> _redactedKeys;

    // Clients send the Azure Functions key as ?code=..., so it is always redacted.
    public QueryFormatter(IEnumerable<string> additionalRedactedKeys)
        => _redactedKeys = new(additionalRedactedKeys.Append("code"), StringComparer.OrdinalIgnoreCase);

    public string Format(IEnumerable<(string Key, string? Value)> parameters)
        => string.Join(
            "&",
            parameters.Select(parameter => $"{parameter.Key}={(_redactedKeys.Contains(parameter.Key) ? Redacted : parameter.Value)}"));
}

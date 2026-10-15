namespace Bazuzi.AzureFunctionInputOutputLogger;

/// <summary>
/// Configures <c>UseInputOutputLogging</c> and <c>UseAspNetCoreInputOutputLogging</c>.
/// </summary>
public sealed class InputOutputLoggingOptions
{
    /// <summary>
    /// When <see langword="true"/>, only functions marked with <see cref="LogFunctionInputsAttribute"/>,
    /// <see cref="LogFunctionOutputsAttribute"/> or <see cref="LogFunctionInputsAndOutputsAttribute"/> are logged.
    /// </summary>
    public bool RequireMarkerAttribute { get; set; }

    /// <summary>
    /// Query parameters whose values are logged as <c>[redacted]</c>, matched case-insensitively.
    /// <c>code</c>, which carries the Azure Functions key, is always redacted and doesn't need to be added.
    /// </summary>
    public ISet<string> RedactedQueryParameters { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}

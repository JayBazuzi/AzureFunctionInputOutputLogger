namespace AzureFunctionInputOutputLogger;

/// <param name="RequireMarkerAttribute">
/// When true, only functions (or classes) decorated with <see cref="LogInputOutputAttribute"/> are logged.
/// When false, every HTTP-triggered function is logged.
/// </param>
internal sealed record InputOutputLoggingOptions(bool RequireMarkerAttribute);

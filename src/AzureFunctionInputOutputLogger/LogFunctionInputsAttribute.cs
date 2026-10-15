namespace Bazuzi.AzureFunctionInputOutputLogger;

/// <summary>
/// Marks a function (or a class of functions) whose input, but not output, should be logged
/// when <c>requireMarkerAttribute</c> is passed to <c>UseInputOutputLogging</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
public sealed class LogFunctionInputsAttribute : Attribute
{
}

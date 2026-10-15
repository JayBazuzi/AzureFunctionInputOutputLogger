namespace Bazuzi.AzureFunctionInputOutputLogger;

[Flags]
internal enum LoggedParts
{
    None = 0,
    Input = 1,
    Output = 2,
    InputAndOutput = Input | Output,
}

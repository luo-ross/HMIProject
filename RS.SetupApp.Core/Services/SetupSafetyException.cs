namespace RS.SetupApp.Core;

public sealed class SetupSafetyException : InvalidOperationException
{
    public SetupSafetyException(InstallTargetFailureCode failureCode, string message)
        : base(message)
    {
        FailureCode = failureCode;
    }

    public InstallTargetFailureCode FailureCode { get; }
}

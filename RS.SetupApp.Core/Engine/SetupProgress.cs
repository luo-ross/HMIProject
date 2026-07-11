namespace RS.SetupApp.Core;

public sealed class SetupProgress
{
    public Guid OperationId { get; init; }

    public int CurrentStep { get; init; }

    public int TotalSteps { get; init; }

    public string Message { get; init; } = string.Empty;

    public double Percent => TotalSteps == 0 ? 0 : Math.Round((double)CurrentStep / TotalSteps * 100, 2);
}

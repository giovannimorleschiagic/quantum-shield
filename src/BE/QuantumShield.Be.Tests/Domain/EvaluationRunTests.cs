using QuantumShield.Be.Domain.Enums;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Tests.Domain;

public sealed class EvaluationRunTests
{
    [Fact]
    public void Complete_ShouldSetStatusBlobAndTimestamp()
    {
        var run = EvaluationRun.CreatePending(Guid.NewGuid());
        run.MarkInProgress();

        run.Complete("tenant-a/run-a.json");

        Assert.Equal(EvaluationRunStatus.Completed, run.Status);
        Assert.Equal("tenant-a/run-a.json", run.ResultBlobName);
        Assert.NotNull(run.CompletedAtUtc);
    }
}

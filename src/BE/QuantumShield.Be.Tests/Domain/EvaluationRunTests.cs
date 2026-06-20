using QuantumShield.Be.Domain.Enums;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Tests.Domain;

public sealed class EvaluationRunTests
{
    [Fact]
    public void Complete_ShouldSetStatusCountsAndTimestamp()
    {
        var run = EvaluationRun.CreatePending(Guid.NewGuid(), "template-a", "v1");
        run.MarkInProgress();

        run.Complete([
            new EvaluationResult("rule-1", "Rule 1", EvaluationCheckStatus.Passed, EvaluationSeverity.Low, "enabled", "enabled", null),
            new EvaluationResult("rule-2", "Rule 2", EvaluationCheckStatus.Failed, EvaluationSeverity.High, "enabled", "disabled", null)
        ]);

        Assert.Equal(EvaluationRunStatus.Completed, run.Status);
        Assert.Equal(2, run.TotalChecks);
        Assert.Equal(1, run.PassedChecks);
        Assert.Equal(1, run.FailedChecks);
        Assert.NotNull(run.CompletedAtUtc);
    }
    
    
}

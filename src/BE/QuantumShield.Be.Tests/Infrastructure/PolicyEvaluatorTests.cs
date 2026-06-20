using QuantumShield.Be.Domain.Enums;
using QuantumShield.Be.Domain.Models;
using QuantumShield.Be.Infrastructure.Evaluation;

namespace QuantumShield.Be.Tests.Infrastructure;

public sealed class PolicyEvaluatorTests
{
    [Fact]
    public void Evaluate_ShouldCompareSnapshotValues()
    {
        var template = new EvaluationTemplateDefinition(
            "template-a",
            "v1",
            [new EvaluationRuleDefinition("rule-1", "Rule 1", "organization.displayName", EvaluationComparisonType.Equals, EvaluationSeverity.Low, "Contoso")]);
        var snapshot = new TenantEvaluationSnapshot(new Dictionary<string, string?> { ["organization.displayName"] = "Contoso" });

        var results = new PolicyEvaluator().Evaluate(template, snapshot);

        Assert.Single(results);
        Assert.Equal(EvaluationCheckStatus.Passed, results.Single().Status);
    }
}

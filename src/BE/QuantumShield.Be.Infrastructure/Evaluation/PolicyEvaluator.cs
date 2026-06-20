using QuantumShield.Be.Domain.Enums;
using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Infrastructure.Evaluation;

public sealed class PolicyEvaluator : IPolicyEvaluator
{
    public IReadOnlyCollection<EvaluationResult> Evaluate(EvaluationTemplateDefinition template, TenantEvaluationSnapshot snapshot)
    {
        return template.Rules.Select(rule =>
        {
            snapshot.Values.TryGetValue(rule.DataPath, out var actualValue);

            var status = rule.ComparisonType switch
            {
                EvaluationComparisonType.Equals => string.Equals(actualValue, rule.ExpectedValue, StringComparison.OrdinalIgnoreCase)
                    ? EvaluationCheckStatus.Passed
                    : EvaluationCheckStatus.Failed,
                EvaluationComparisonType.NotEquals => !string.Equals(actualValue, rule.ExpectedValue, StringComparison.OrdinalIgnoreCase)
                    ? EvaluationCheckStatus.Passed
                    : EvaluationCheckStatus.Failed,
                EvaluationComparisonType.Contains => actualValue?.Contains(rule.ExpectedValue ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true
                    ? EvaluationCheckStatus.Passed
                    : EvaluationCheckStatus.Failed,
                EvaluationComparisonType.Exists => string.IsNullOrWhiteSpace(actualValue)
                    ? EvaluationCheckStatus.Failed
                    : EvaluationCheckStatus.Passed,
                _ => EvaluationCheckStatus.NotApplicable
            };

            return new EvaluationResult(
                rule.Key,
                rule.DisplayName,
                status,
                rule.Severity,
                rule.ExpectedValue,
                actualValue,
                $"Compared '{rule.DataPath}' using '{rule.ComparisonType}'.");
        }).ToList();
    }
}

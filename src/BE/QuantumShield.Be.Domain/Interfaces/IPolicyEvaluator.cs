using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Domain.Interfaces;

public interface IPolicyEvaluator
{
    IReadOnlyCollection<EvaluationResult> Evaluate(EvaluationTemplateDefinition template, TenantEvaluationSnapshot snapshot);
}

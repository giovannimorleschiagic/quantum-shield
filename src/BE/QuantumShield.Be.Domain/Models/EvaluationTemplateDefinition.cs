using QuantumShield.Be.Domain.Enums;
using QuantumShield.Be.Domain.Exceptions;

namespace QuantumShield.Be.Domain.Models;

public sealed class EvaluationTemplateDefinition
{
    public EvaluationTemplateDefinition(string templateIdentifier, string? version, IReadOnlyCollection<EvaluationRuleDefinition> rules)
    {
        if (string.IsNullOrWhiteSpace(templateIdentifier))
        {
            throw new DomainValidationException("Template identifier is required.");
        }

        if (rules.Count == 0)
        {
            throw new DomainValidationException("At least one evaluation rule is required.");
        }

        TemplateIdentifier = templateIdentifier.Trim();
        Version = version?.Trim();
        Rules = rules;
    }

    public string TemplateIdentifier { get; }

    public string? Version { get; }

    public IReadOnlyCollection<EvaluationRuleDefinition> Rules { get; }
}

public sealed class EvaluationRuleDefinition
{
    public EvaluationRuleDefinition(
        string key,
        string displayName,
        string dataPath,
        EvaluationComparisonType comparisonType,
        EvaluationSeverity severity,
        string? expectedValue)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new DomainValidationException("Rule key is required.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainValidationException("Rule display name is required.");
        }

        if (string.IsNullOrWhiteSpace(dataPath))
        {
            throw new DomainValidationException("Rule data path is required.");
        }

        Key = key.Trim();
        DisplayName = displayName.Trim();
        DataPath = dataPath.Trim();
        ComparisonType = comparisonType;
        Severity = severity;
        ExpectedValue = expectedValue?.Trim();
    }

    public string Key { get; }

    public string DisplayName { get; }

    public string DataPath { get; }

    public EvaluationComparisonType ComparisonType { get; }

    public EvaluationSeverity Severity { get; }

    public string? ExpectedValue { get; }
}

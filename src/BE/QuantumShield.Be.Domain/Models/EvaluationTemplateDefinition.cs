using QuantumShield.Be.Domain.Exceptions;

namespace QuantumShield.Be.Domain.Models;

public sealed class EvaluationTemplateDefinition
{
    public EvaluationTemplateDefinition(
        string controlId,
        string benchmark,
        string? version,
        string section,
        string title,
        IReadOnlyCollection<EvaluationCheckDefinition> checks)
    {
        if (string.IsNullOrWhiteSpace(controlId))
        {
            throw new DomainValidationException("Control id is required.");
        }

        if (string.IsNullOrWhiteSpace(benchmark))
        {
            throw new DomainValidationException("Benchmark is required.");
        }

        if (string.IsNullOrWhiteSpace(section))
        {
            throw new DomainValidationException("Section is required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Title is required.");
        }

        ControlId = controlId.Trim();
        Benchmark = benchmark.Trim();
        Version = version?.Trim();
        Section = section.Trim();
        Title = title.Trim();
        Checks = checks;
    }

    public string ControlId { get; }

    public string Benchmark { get; }

    public string? Version { get; }

    public string Section { get; }

    public string Title { get; }

    public IReadOnlyCollection<EvaluationCheckDefinition> Checks { get; }

    public IReadOnlyCollection<EvaluationCheckDefinition> AutomaticChecks
        => Checks.Where(static check => check.IsAutomatic).ToList();
}

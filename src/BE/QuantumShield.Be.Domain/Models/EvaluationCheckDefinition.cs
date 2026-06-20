using QuantumShield.Be.Domain.Exceptions;

namespace QuantumShield.Be.Domain.Models;

public sealed class EvaluationCheckDefinition
{
    public EvaluationCheckDefinition(
        string checkId,
        string description,
        string method,
        string? endpoint,
        IReadOnlyCollection<string> graphPermissions,
        string expectedResult,
        bool isSupportedForB2C = true)
    {
        if (string.IsNullOrWhiteSpace(checkId))
        {
            throw new DomainValidationException("Check id is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainValidationException("Check description is required.");
        }

        if (string.IsNullOrWhiteSpace(method))
        {
            throw new DomainValidationException("Check method is required.");
        }

        if (string.IsNullOrWhiteSpace(expectedResult))
        {
            throw new DomainValidationException("Check expected result is required.");
        }

        CheckId = checkId.Trim();
        Description = description.Trim();
        Method = method.Trim();
        Endpoint = endpoint?.Trim();
        GraphPermissions = graphPermissions;
        ExpectedResult = expectedResult.Trim();
        IsSupportedForB2C = isSupportedForB2C;
    }

    public string CheckId { get; }

    public string Description { get; }

    public string Method { get; }

    public string? Endpoint { get; }

    public IReadOnlyCollection<string> GraphPermissions { get; }

    public string ExpectedResult { get; }

    public bool IsSupportedForB2C { get; }

    public bool IsAutomatic => string.Equals(Method, "graph_api", StringComparison.OrdinalIgnoreCase);
}

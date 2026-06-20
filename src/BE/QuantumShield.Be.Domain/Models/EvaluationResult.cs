using QuantumShield.Be.Domain.Enums;
using QuantumShield.Be.Domain.Exceptions;

namespace QuantumShield.Be.Domain.Models;

public sealed class EvaluationResult
{
    public EvaluationResult(
        string controlId,
        string checkId,
        string title,
        string description,
        string method,
        string endpoint,
        IReadOnlyCollection<string> graphPermissions,
        string expectedResult,
        EvaluationCheckStatus status,
        string? actualResult,
        string? rawResult,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(controlId))
        {
            throw new DomainValidationException("Control id is required.");
        }

        if (string.IsNullOrWhiteSpace(checkId))
        {
            throw new DomainValidationException("Check id is required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainValidationException("Description is required.");
        }

        if (string.IsNullOrWhiteSpace(method))
        {
            throw new DomainValidationException("Method is required.");
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new DomainValidationException("Endpoint is required.");
        }

        if (string.IsNullOrWhiteSpace(expectedResult))
        {
            throw new DomainValidationException("Expected result is required.");
        }

        ControlId = controlId.Trim();
        CheckId = checkId.Trim();
        Title = title.Trim();
        Description = description.Trim();
        Method = method.Trim();
        Endpoint = endpoint.Trim();
        GraphPermissions = graphPermissions;
        ExpectedResult = expectedResult.Trim();
        Status = status;
        ActualResult = actualResult?.Trim();
        RawResult = rawResult;
        Notes = notes?.Trim();
    }

    public string ControlId { get; }

    public string CheckId { get; }

    public string Title { get; }

    public string Description { get; }

    public string Method { get; }

    public string Endpoint { get; }

    public IReadOnlyCollection<string> GraphPermissions { get; }

    public string ExpectedResult { get; }

    public EvaluationCheckStatus Status { get; }

    public string? ActualResult { get; }

    public string? RawResult { get; }

    public string? Notes { get; }
}

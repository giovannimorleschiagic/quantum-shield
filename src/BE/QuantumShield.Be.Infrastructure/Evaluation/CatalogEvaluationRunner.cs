using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.Identity;
using Microsoft.Extensions.Options;
using QuantumShield.Be.Domain.Enums;
using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;
using QuantumShield.Be.Domain.Options;

namespace QuantumShield.Be.Infrastructure.Evaluation;

public sealed partial class CatalogEvaluationRunner : ICatalogEvaluationRunner
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly Regex EqualityRegex = EqualityPattern();
    private static readonly Regex GraphRequestRegex = GraphRequestPattern();
    private readonly GraphOptions _graphOptions;

    public CatalogEvaluationRunner(IOptions<GraphOptions> graphOptions)
    {
        _graphOptions = graphOptions.Value;
    }

    public async Task<EvaluationArtifactDocument> RunAsync(
        Tenant tenant,
        string clientSecret,
        Guid evaluationId,
        DateTimeOffset startedAtUtc,
        IReadOnlyCollection<EvaluationTemplateDefinition> templates,
        CancellationToken cancellationToken)
    {
        var credential = new ClientSecretCredential(tenant.TenantId, tenant.ClientId, clientSecret);
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
        };

        var accessToken = await credential.GetTokenAsync(
            new Azure.Core.TokenRequestContext(_graphOptions.Scopes),
            cancellationToken);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

        var templateResults = new List<EvaluationArtifactTemplateResult>();
        var templatesSkipped = 0;

        foreach (var template in templates.OrderBy(static item => item.ControlId, StringComparer.OrdinalIgnoreCase))
        {
            var automaticChecks = template.AutomaticChecks.ToList();
            if (automaticChecks.Count == 0)
            {
                templatesSkipped++;
                continue;
            }

            var checks = new List<EvaluationCheckResult>(automaticChecks.Count);
            foreach (var check in automaticChecks)
            {
                checks.Add(await ExecuteCheckAsync(httpClient, template, check, cancellationToken));
            }

            templateResults.Add(new EvaluationArtifactTemplateResult(
                template.ControlId,
                template.Benchmark,
                template.Version,
                template.Section,
                template.Title,
                checks));
        }

        var allChecks = templateResults.SelectMany(static item => item.Checks).ToList();
        var summary = new EvaluationArtifactSummary(
            allChecks.Count,
            allChecks.Count(static check => check.Status == EvaluationCheckStatus.Passed),
            allChecks.Count(static check => check.Status == EvaluationCheckStatus.Failed),
            allChecks.Count(static check => check.Status == EvaluationCheckStatus.NotApplicable),
            templateResults.Count,
            templatesSkipped);

        return new EvaluationArtifactDocument(
            evaluationId,
            tenant.Id,
            startedAtUtc,
            DateTimeOffset.UtcNow,
            EvaluationRunStatus.Completed,
            summary,
            templateResults);
    }

    private static async Task<EvaluationCheckResult> ExecuteCheckAsync(
        HttpClient httpClient,
        EvaluationTemplateDefinition template,
        EvaluationCheckDefinition check,
        CancellationToken cancellationToken)
    {
        var requestUris = ExtractRequestUris(check.Endpoint);
        if (requestUris.Count == 0)
        {
            return ToNotApplicable(template, check, "The template check does not define a Graph endpoint.");
        }

        var payloads = new List<string>(requestUris.Count);
        foreach (var requestUri in requestUris)
        {
            using var response = await httpClient.GetAsync(requestUri, cancellationToken);
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
            payloads.Add(rawContent);

            if (!response.IsSuccessStatusCode)
            {
                return new EvaluationCheckResult(
                    template.ControlId,
                    check.CheckId,
                    template.Title,
                    check.Description,
                    check.Method,
                    check.Endpoint ?? string.Empty,
                    check.GraphPermissions,
                    check.ExpectedResult,
                    EvaluationCheckStatus.Failed,
                    $"HTTP {(int)response.StatusCode}",
                    rawContent,
                    $"Graph endpoint call failed for '{requestUri}'.");
            }
        }

        var combinedRawContent = payloads.Count == 1
            ? payloads[0]
            : JsonSerializer.Serialize(payloads, SerializerOptions);

        return Evaluate(template, check, combinedRawContent);
    }

    private static EvaluationCheckResult Evaluate(
        EvaluationTemplateDefinition template,
        EvaluationCheckDefinition check,
        string rawContent)
    {
        try
        {
            using var document = JsonDocument.Parse(rawContent);
            if (TryEvaluateEqualsExpression(document.RootElement, check.ExpectedResult, out var equalsResult))
            {
                return new EvaluationCheckResult(
                    template.ControlId,
                    check.CheckId,
                    template.Title,
                    check.Description,
                    check.Method,
                    check.Endpoint ?? string.Empty,
                    check.GraphPermissions,
                    check.ExpectedResult,
                    equalsResult.status,
                    equalsResult.actual,
                    rawContent,
                    equalsResult.notes);
            }

            if (TryEvaluateCountExpression(document.RootElement, check.ExpectedResult, out var countResult))
            {
                return new EvaluationCheckResult(
                    template.ControlId,
                    check.CheckId,
                    template.Title,
                    check.Description,
                    check.Method,
                    check.Endpoint ?? string.Empty,
                    check.GraphPermissions,
                    check.ExpectedResult,
                    countResult.status,
                    countResult.actual,
                    rawContent,
                    countResult.notes);
            }

            if (TryEvaluateContainsExpression(rawContent, check.ExpectedResult, out var containsResult))
            {
                return new EvaluationCheckResult(
                    template.ControlId,
                    check.CheckId,
                    template.Title,
                    check.Description,
                    check.Method,
                    check.Endpoint ?? string.Empty,
                    check.GraphPermissions,
                    check.ExpectedResult,
                    containsResult.status,
                    containsResult.actual,
                    rawContent,
                    containsResult.notes);
            }
        }
        catch (JsonException)
        {
            return new EvaluationCheckResult(
                template.ControlId,
                check.CheckId,
                template.Title,
                check.Description,
                check.Method,
                check.Endpoint ?? string.Empty,
                check.GraphPermissions,
                check.ExpectedResult,
                EvaluationCheckStatus.NotApplicable,
                "Response is not JSON.",
                rawContent,
                "The Graph response could not be interpreted as JSON.");
        }

        return new EvaluationCheckResult(
            template.ControlId,
            check.CheckId,
            template.Title,
            check.Description,
            check.Method,
            check.Endpoint ?? string.Empty,
            check.GraphPermissions,
            check.ExpectedResult,
            EvaluationCheckStatus.NotApplicable,
            "Automatic semantic comparison unavailable.",
            rawContent,
            "The raw Graph response was captured, but the expected result requires control-specific interpretation.");
    }

    private static IReadOnlyCollection<string> ExtractRequestUris(string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return [];
        }

        var requestUris = new List<string>();
        foreach (Match match in GraphRequestRegex.Matches(endpoint))
        {
            var normalized = NormalizeRequestUri(match.Groups["uri"].Value);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                requestUris.Add(normalized);
            }
        }

        return requestUris;
    }

    private static string NormalizeRequestUri(string requestUri)
    {
        var normalized = requestUri.Trim().TrimStart('/');
        normalized = Regex.Replace(normalized, @"\s+(?:oppure|via)\s+.*$", string.Empty, RegexOptions.IgnoreCase);

        if (normalized.Contains("/policies/roleManagementPolicies/{", StringComparison.OrdinalIgnoreCase)
            && normalized.Contains("}/rules", StringComparison.OrdinalIgnoreCase))
        {
            normalized = Regex.Replace(normalized, @"/\{[^}]+\}/rules", string.Empty, RegexOptions.IgnoreCase);
        }
        else
        {
            normalized = Regex.Replace(normalized, @"/\{[^}]+\}", string.Empty, RegexOptions.IgnoreCase);
        }

        normalized = Regex.Replace(normalized, @"([?&][^=]+)=([^&]*\{[^}]+\}[^&]*)", string.Empty, RegexOptions.IgnoreCase);
        normalized = normalized.Replace("?&", "?", StringComparison.Ordinal);
        normalized = normalized.TrimEnd('&', '?');
        normalized = normalized.Replace(" ", "%20", StringComparison.Ordinal)
            .Replace("'", "%27", StringComparison.Ordinal);
        return normalized;
    }

    private static EvaluationCheckResult ToNotApplicable(
        EvaluationTemplateDefinition template,
        EvaluationCheckDefinition check,
        string reason)
        => new(
            template.ControlId,
            check.CheckId,
            template.Title,
            check.Description,
            check.Method,
            check.Endpoint ?? "N/A",
            check.GraphPermissions,
            check.ExpectedResult,
            EvaluationCheckStatus.NotApplicable,
            null,
            null,
            reason);

    private static bool TryEvaluateEqualsExpression(JsonElement rootElement, string expectedResult, out (EvaluationCheckStatus status, string actual, string notes) result)
    {
        var match = EqualityRegex.Match(expectedResult);
        if (!match.Success)
        {
            result = default;
            return false;
        }

        var propertyName = match.Groups["property"].Value;
        var expectedValue = match.Groups["value"].Value.Trim();
        var actualValues = FindPropertyValues(rootElement, propertyName).ToList();
        if (actualValues.Count == 0)
        {
            result = (EvaluationCheckStatus.Failed, $"Property '{propertyName}' not found.", "The expected property was not present in the Graph response.");
            return true;
        }

        var expectedNormalized = expectedValue.Trim('\'', '"');
        var passed = actualValues.Any(value => string.Equals(NormalizeJsonValue(value), expectedNormalized, StringComparison.OrdinalIgnoreCase));
        result = (
            passed ? EvaluationCheckStatus.Passed : EvaluationCheckStatus.Failed,
            string.Join("; ", actualValues.Select(NormalizeJsonValue)),
            $"Compared property '{propertyName}' with expected value '{expectedNormalized}'.");
        return true;
    }

    private static bool TryEvaluateCountExpression(JsonElement rootElement, string expectedResult, out (EvaluationCheckStatus status, string actual, string notes) result)
    {
        const string marker = ">=";
        if (!expectedResult.Contains(marker, StringComparison.Ordinal))
        {
            result = default;
            return false;
        }

        var numberMatch = Regex.Match(expectedResult, @">=\s*(?<count>\d+)");
        if (!numberMatch.Success)
        {
            result = default;
            return false;
        }

        var minimum = int.Parse(numberMatch.Groups["count"].Value);
        var count = rootElement.ValueKind switch
        {
            JsonValueKind.Array => rootElement.GetArrayLength(),
            JsonValueKind.Object when rootElement.TryGetProperty("value", out var valueElement) && valueElement.ValueKind == JsonValueKind.Array => valueElement.GetArrayLength(),
            _ => 0
        };

        result = (
            count >= minimum ? EvaluationCheckStatus.Passed : EvaluationCheckStatus.Failed,
            count.ToString(),
            $"Compared result count against minimum '{minimum}'.");
        return true;
    }

    private static bool TryEvaluateContainsExpression(string rawContent, string expectedResult, out (EvaluationCheckStatus status, string actual, string notes) result)
    {
        var tokens = Regex.Matches(expectedResult, @"'(?<token>[^']+)'")
            .Select(static match => match.Groups["token"].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (tokens.Count == 0)
        {
            result = default;
            return false;
        }

        var foundTokens = tokens.Where(token => rawContent.Contains(token, StringComparison.OrdinalIgnoreCase)).ToList();
        result = (
            foundTokens.Count > 0 ? EvaluationCheckStatus.Passed : EvaluationCheckStatus.Failed,
            foundTokens.Count == 0 ? "No expected token found." : string.Join(", ", foundTokens),
            "Compared the raw Graph response with the expected token list.");
        return true;
    }

    private static IEnumerable<JsonElement> FindPropertyValues(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    yield return property.Value;
                }

                foreach (var nested in FindPropertyValues(property.Value, propertyName))
                {
                    yield return nested;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                foreach (var nested in FindPropertyValues(item, propertyName))
                {
                    yield return nested;
                }
            }
        }
    }

    private static string NormalizeJsonValue(JsonElement value)
        => value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.True => bool.TrueString.ToLowerInvariant(),
            JsonValueKind.False => bool.FalseString.ToLowerInvariant(),
            JsonValueKind.Null => "null",
            _ => value.ToString()
        };

    [GeneratedRegex(@"(?<property>[A-Za-z0-9_.]+)\s*=\s*(?<value>true|false|null|'[^']+'|""[^""]+""|[A-Za-z0-9_.-]+)", RegexOptions.IgnoreCase)]
    private static partial Regex EqualityPattern();

    [GeneratedRegex(@"GET\s+(?<uri>/.*?)(?=(?:\s+-\s+|\s+Alternativa:|\s+oppure\s+|$|(?=\s+e\s+GET\s+)|(?=\s+GET\s+/)))", RegexOptions.IgnoreCase)]
    private static partial Regex GraphRequestPattern();
}

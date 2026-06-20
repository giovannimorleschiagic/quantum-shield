using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantumShield.Be.Api.Contracts;
using QuantumShield.Be.Domain.Interfaces;

namespace QuantumShield.Be.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class EvaluationRunsController : ControllerBase
{
    private readonly IEvaluationRunService _evaluationRunService;

    public EvaluationRunsController(IEvaluationRunService evaluationRunService)
    {
        _evaluationRunService = evaluationRunService;
    }

    [HttpPost("evaluations/runs")]
    public async Task<ActionResult<EvaluationRunSummaryResponse>> Trigger(TriggerEvaluationRunRequest request, CancellationToken cancellationToken)
    {
        var run = await _evaluationRunService.TriggerAsync(request.TenantId, cancellationToken);
        return AcceptedAtAction(nameof(GetById), new { runId = run.Id }, run.ToSummaryResponse());
    }

    [HttpGet("evaluations/runs")]
    public async Task<ActionResult<IReadOnlyCollection<EvaluationRunSummaryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var runs = await _evaluationRunService.ListAsync(cancellationToken);
        return Ok(runs.Select(static item => item.ToSummaryResponse()).ToList());
    }

    [HttpGet("evaluations/runs/{runId:guid}")]
    public async Task<ActionResult<EvaluationRunDetailResponse>> GetById(Guid runId, CancellationToken cancellationToken)
    {
        var run = await _evaluationRunService.GetByIdAsync(runId, cancellationToken);
        if (run is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(run.ResultBlobName) && run.Artifact is null)
        {
            return Problem(
                title: "Evaluation artifact unavailable",
                detail: $"The evaluation artifact '{run.ResultBlobName}' could not be loaded for run '{run.Id}'.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(run.ToDetailResponse());
    }

    [HttpGet("tenants/{tenantId:guid}/runs")]
    public async Task<ActionResult<IReadOnlyCollection<EvaluationRunSummaryResponse>>> GetByTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var runs = await _evaluationRunService.ListByTenantAsync(tenantId, cancellationToken);
        return Ok(runs.Select(static item => item.ToSummaryResponse()).ToList());
    }
}

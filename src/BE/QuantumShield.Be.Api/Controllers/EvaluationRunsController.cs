using Microsoft.AspNetCore.Mvc;
using QuantumShield.Be.Api.Contracts;
using QuantumShield.Be.Business.Services;

namespace QuantumShield.Be.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class EvaluationRunsController : ControllerBase
{
    private readonly EvaluationRunService _evaluationRunService;

    public EvaluationRunsController(EvaluationRunService evaluationRunService)
    {
        _evaluationRunService = evaluationRunService;
    }

    [HttpPost("evaluations/runs")]
    public async Task<ActionResult<EvaluationRunResponse>> Trigger(TriggerEvaluationRunRequest request, CancellationToken cancellationToken)
    {
        var run = await _evaluationRunService.TriggerAsync(request.TenantId, request.TemplateIdentifier, cancellationToken);
        return AcceptedAtAction(nameof(GetById), new { runId = run.Id }, run.ToResponse());
    }

    [HttpGet("evaluations/runs")]
    public async Task<ActionResult<IReadOnlyCollection<EvaluationRunResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var runs = await _evaluationRunService.ListAsync(cancellationToken);
        return Ok(runs.Select(static item => item.ToResponse()).ToList());
    }

    [HttpGet("evaluations/runs/{runId:guid}")]
    public async Task<ActionResult<EvaluationRunResponse>> GetById(Guid runId, CancellationToken cancellationToken)
    {
        var run = await _evaluationRunService.GetByIdAsync(runId, cancellationToken);
        return run is null ? NotFound() : Ok(run.ToResponse());
    }

    [HttpGet("tenants/{tenantId:guid}/runs")]
    public async Task<ActionResult<IReadOnlyCollection<EvaluationRunResponse>>> GetByTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var runs = await _evaluationRunService.ListByTenantAsync(tenantId, cancellationToken);
        return Ok(runs.Select(static item => item.ToResponse()).ToList());
    }
}

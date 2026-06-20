using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantumShield.Be.Api.Contracts;
using QuantumShield.Be.Domain.Exceptions;
using QuantumShield.Be.Domain.Interfaces;

namespace QuantumShield.Be.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tenants")]
public sealed class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TenantResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var tenants = await _tenantService.ListAsync(cancellationToken);
        return Ok(tenants.Select(static item => item.ToResponse()).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TenantResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _tenantService.GetByIdAsync(id, cancellationToken);
        return tenant is null ? NotFound() : Ok(tenant.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<TenantResponse>> Create(CreateTenantRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _tenantService.CreateAsync(
                request.TenantName,
                request.TenantId,
                request.ClientId,
                request.ClientSecret,
                request.IsActive,
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, tenant.ToResponse());
        }
        catch (DomainValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails
            {
                Detail = exception.Message
            });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TenantResponse>> Update(Guid id, UpdateTenantRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _tenantService.UpdateAsync(
                id,
                request.TenantName,
                request.TenantId,
                request.ClientId,
                request.ClientSecret,
                request.IsActive,
                cancellationToken);

            return tenant is null ? NotFound() : Ok(tenant.ToResponse());
        }
        catch (DomainValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails
            {
                Detail = exception.Message
            });
        }
    }
}

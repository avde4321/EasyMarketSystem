using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Proformas.Ports.In;
using TestDeIa.Shared.Requests.Proformas;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/proformas")]
public sealed class ProformasController(IProformaService proformaService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? term,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 25);
        skip = Math.Max(0, skip);
        return Ok(await proformaService.GetPagedAsync(term, skip, take, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var proforma = await proformaService.GetByIdAsync(id, cancellationToken);
        return proforma is null ? NotFound() : Ok(proforma);
    }

    [HttpPost]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> Create([FromBody] ProformaRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await proformaService.CreateAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProformaRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var proforma = await proformaService.UpdateAsync(id, request, cancellationToken);
            return proforma is null ? NotFound() : Ok(proforma);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/anular")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> Anular(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await proformaService.AnularAsync(id, cancellationToken) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/convertir-a-factura")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> ConvertirAFactura(
        Guid id,
        [FromBody] ConvertirProformaAFacturaDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            request.ProformaId = id;
            return Accepted(await proformaService.ConvertirProformaAFacturaAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}

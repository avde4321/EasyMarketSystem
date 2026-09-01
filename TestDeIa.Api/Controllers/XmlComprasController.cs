using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.XmlSri.Ports.In;
using TestDeIa.Shared.Requests.XmlSri;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/xml-compras")]
public sealed class XmlComprasController(
    IXmlSriParserService xmlSriParserService,
    ITenantContextAccessor tenantContextAccessor) : ControllerBase
{
    [HttpPost("cargar-masivo")]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    [RequestSizeLimit(30_000_000)]
    public async Task<IActionResult> CargarMasivo(IReadOnlyCollection<IFormFile> archivos, CancellationToken cancellationToken)
    {
        if (archivos.Count == 0)
        {
            return BadRequest(new { message = "Debes enviar al menos un archivo XML o ZIP." });
        }

        var xmls = new List<string>();
        foreach (var archivo in archivos)
        {
            if (archivo.Length == 0)
            {
                continue;
            }

            await using var stream = archivo.OpenReadStream();
            if (archivo.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
                foreach (var entry in archive.Entries.Where(current => current.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
                {
                    await using var entryStream = entry.Open();
                    using var reader = new StreamReader(entryStream);
                    xmls.Add(await reader.ReadToEndAsync(cancellationToken));
                }

                continue;
            }

            using var fileReader = new StreamReader(stream);
            xmls.Add(await fileReader.ReadToEndAsync(cancellationToken));
        }

        var empresaId = tenantContextAccessor.EmpresaId
            ?? throw new InvalidOperationException("No existe una empresa activa para importar XML de compras.");

        return Ok(await xmlSriParserService.ProcesarArchivosXmlMasivosAsync(xmls, empresaId, cancellationToken));
    }

    [HttpGet("pendientes")]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    public async Task<IActionResult> GetPendientes([FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 50);
        skip = Math.Max(0, skip);
        return Ok(await xmlSriParserService.GetPendientesAsync(skip, take, cancellationToken));
    }

    [HttpPost("{id:guid}/convertir-compra")]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    public async Task<IActionResult> ConvertirCompra(Guid id, [FromBody] ConvertirXmlACompraRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.FacturaCompraXmlLogId = id;
            return Ok(await xmlSriParserService.ConvertirXmlACompraAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpGet("{id:guid}/parseado")]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    public async Task<IActionResult> GetParseado(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await xmlSriParserService.GetParsedAsync(id, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpGet("{id:guid}/xml-original")]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    public async Task<IActionResult> GetXmlOriginal(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var xml = await xmlSriParserService.GetXmlOriginalAsync(id, cancellationToken);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", $"XML-COMPRA-{id:N}.xml");
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}

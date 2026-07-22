using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.ActivosFijos.Ports.In;
using TestDeIa.Application.Modules.ActivosFijos.Ports.Out;
using TestDeIa.Domain.Modules.ActivosFijos.Entities;
using DomainCategoria = TestDeIa.Domain.Modules.ActivosFijos.Enums.CategoriaSriActivoFijo;
using DomainEstado = TestDeIa.Domain.Modules.ActivosFijos.Enums.EstadoActivoFijo;
using SharedCategoria = TestDeIa.Shared.ActivosFijos.CategoriaSriActivoFijo;
using SharedEstado = TestDeIa.Shared.ActivosFijos.EstadoActivoFijo;
using TestDeIa.Shared.Requests.ActivosFijos;
using TestDeIa.Shared.Responses.ActivosFijos;

namespace TestDeIa.Application.Modules.ActivosFijos.UseCases;

public sealed class ActivoFijoUseCase(
    IActivoFijoRepository activoFijoRepository,
    ITenantContextAccessor tenantContextAccessor,
    ActivoFijoService activoFijoService) : IActivoFijoUseCase
{
    public async Task<IReadOnlyCollection<ActivoFijoResponse>> GetAsync(SharedCategoria? categoria, string? custodio, SharedEstado? estado, CancellationToken cancellationToken = default)
    {
        var activos = await activoFijoRepository.GetAsync(
            categoria.HasValue ? ToDomainCategoria(categoria.Value) : null,
            custodio,
            estado.HasValue ? ToDomainEstado(estado.Value) : null,
            cancellationToken);

        return activos.Select(Map).ToArray();
    }

    public async Task<ActivoFijoResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var activo = await activoFijoRepository.GetByIdAsync(id, cancellationToken);
        return activo is null ? null : Map(activo);
    }

    public async Task<ActivoFijoResponse> CreateAsync(ActivoFijoRequest request, CancellationToken cancellationToken = default)
    {
        var empresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para registrar activos fijos.");
        var categoria = ToDomainCategoria(request.CategoriaSRI);
        var parametros = activoFijoService.GetParametrosSri(categoria);
        var codigo = await activoFijoRepository.ReserveNextCodigoAsync(request.FechaAdquisicion, cancellationToken);

        var activo = new ActivoFijo(
            Guid.NewGuid(),
            empresaId,
            null,
            codigo,
            NormalizeRequired(request.Nombre, "El nombre del activo fijo es obligatorio."),
            NormalizeOptional(request.SerieMarca),
            categoria,
            request.FechaAdquisicion.Date,
            request.CostoInicial,
            request.ValorResidual,
            parametros.VidaUtilAnios,
            parametros.PorcentajeDepreciacionAnual,
            NormalizeOptional(request.UbicacionFisica),
            NormalizeOptional(request.CustodioResponsable),
            ToDomainEstado(request.EstadoActivo),
            DateTimeOffset.UtcNow,
            null);

        return Map(await activoFijoRepository.CreateAsync(activo, cancellationToken));
    }

    public async Task<ActivoFijoResponse?> UpdateAsync(Guid id, ActivoFijoRequest request, CancellationToken cancellationToken = default)
    {
        var current = await activoFijoRepository.GetByIdAsync(id, cancellationToken);
        if (current is null)
        {
            return null;
        }

        var categoria = ToDomainCategoria(request.CategoriaSRI);
        var parametros = activoFijoService.GetParametrosSri(categoria);
        var activo = new ActivoFijo(
            current.Id,
            current.EmpresaId,
            current.CompraDetalleId,
            current.CodigoActivo,
            NormalizeRequired(request.Nombre, "El nombre del activo fijo es obligatorio."),
            NormalizeOptional(request.SerieMarca),
            categoria,
            request.FechaAdquisicion.Date,
            request.CostoInicial,
            request.ValorResidual,
            parametros.VidaUtilAnios,
            parametros.PorcentajeDepreciacionAnual,
            NormalizeOptional(request.UbicacionFisica),
            NormalizeOptional(request.CustodioResponsable),
            ToDomainEstado(request.EstadoActivo),
            current.CreatedAt,
            DateTimeOffset.UtcNow);

        var updated = await activoFijoRepository.UpdateAsync(activo, cancellationToken);
        return updated is null ? null : Map(updated);
    }

    private static ActivoFijoResponse Map(ActivoFijo activo) => new()
    {
        Id = activo.Id,
        CompraDetalleId = activo.CompraDetalleId,
        CodigoActivo = activo.CodigoActivo,
        Nombre = activo.Nombre,
        SerieMarca = activo.SerieMarca,
        CategoriaSRI = activo.CategoriaSRI.ToString(),
        FechaAdquisicion = activo.FechaAdquisicion,
        CostoInicial = activo.CostoInicial,
        ValorResidual = activo.ValorResidual,
        VidaUtilAnios = activo.VidaUtilAnios,
        PorcentajeDepreciacionAnual = activo.PorcentajeDepreciacionAnual,
        UbicacionFisica = activo.UbicacionFisica,
        CustodioResponsable = activo.CustodioResponsable,
        EstadoActivo = activo.EstadoActivo.ToString()
    };

    private static DomainCategoria ToDomainCategoria(SharedCategoria categoria) => categoria switch
    {
        SharedCategoria.MaquinariaEquipo => DomainCategoria.MaquinariaEquipo,
        SharedCategoria.Vehiculos => DomainCategoria.Vehiculos,
        SharedCategoria.Edificios => DomainCategoria.Edificios,
        SharedCategoria.MueblesEnseres => DomainCategoria.MueblesEnseres,
        _ => DomainCategoria.EquiposComputo
    };

    private static DomainEstado ToDomainEstado(SharedEstado estado) => estado switch
    {
        SharedEstado.EnMantenimiento => DomainEstado.EnMantenimiento,
        SharedEstado.DadoDeBaja => DomainEstado.DadoDeBaja,
        SharedEstado.DepreciadoTotal => DomainEstado.DepreciadoTotal,
        _ => DomainEstado.Activo
    };

    private static string NormalizeRequired(string? value, string message)
    {
        return string.IsNullOrWhiteSpace(value) ? throw new InvalidOperationException(message) : value.Trim();
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

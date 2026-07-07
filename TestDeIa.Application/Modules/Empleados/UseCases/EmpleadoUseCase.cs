using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Catalogos.Ports.Out;
using TestDeIa.Application.Modules.Empleados.Ports.In;
using TestDeIa.Application.Modules.Empleados.Ports.Out;
using TestDeIa.Application.Modules.Personas.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Empleados.Entities;
using TestDeIa.Domain.Modules.Personas.Entities;
using TestDeIa.Shared.Requests.Empleados;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Empleados;

namespace TestDeIa.Application.Modules.Empleados.UseCases;

public sealed class EmpleadoUseCase : IEmpleadoUseCase
{
    private readonly IEmpleadoRepository empleadoRepository;
    private readonly IPersonaRepository personaRepository;
    private readonly ICatalogoRepository catalogoRepository;
    private readonly ICurrentUserAccessor currentUserAccessor;

    public EmpleadoUseCase(
        IEmpleadoRepository empleadoRepository,
        IPersonaRepository personaRepository,
        ICatalogoRepository catalogoRepository,
        ICurrentUserAccessor currentUserAccessor)
    {
        this.empleadoRepository = empleadoRepository;
        this.personaRepository = personaRepository;
        this.catalogoRepository = catalogoRepository;
        this.currentUserAccessor = currentUserAccessor;
    }

    public async Task<IReadOnlyCollection<EmpleadoResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var empleados = await empleadoRepository.GetAllAsync(cancellationToken);
        return empleados.Select(MapToResponse).ToArray();
    }

    public async Task<PagedResultResponse<EmpleadoResponse>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var page = await empleadoRepository.GetPagedAsync(term, skip, take, cancellationToken);
        return new PagedResultResponse<EmpleadoResponse>
        {
            Items = page.Items.Select(MapToResponse).ToArray(),
            TotalCount = page.TotalCount,
            Skip = page.Skip,
            Take = page.Take
        };
    }

    public async Task<EmpleadoResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var empleado = await empleadoRepository.GetByIdAsync(id, cancellationToken);
        return empleado is null ? null : MapToResponse(empleado);
    }

    public async Task<EmpleadoResponse> CreateAsync(EmpleadoRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogValuesAsync(request, cancellationToken);
        EcuadorIdentificationValidator.EnsureValid(request.TipoIdentificacion, request.Identificacion, "el empleado");

        var persona = await personaRepository.FindByIdentificacionAsync(request.Identificacion, cancellationToken);
        if (persona is not null)
        {
            var existingEmpleado = await empleadoRepository.GetByPersonaIdAsync(persona.Id, cancellationToken: cancellationToken);
            if (existingEmpleado is not null)
            {
                throw new InvalidOperationException("La persona ya tiene el rol de empleado. Puedes editarla desde la lista.");
            }

            persona = await personaRepository.UpdateAsync(BuildPersona(persona.Id, persona.CreatedAt, request), cancellationToken)
                ?? throw new InvalidOperationException("No se pudo actualizar la persona base del empleado.");
        }
        else
        {
            persona = await personaRepository.CreateAsync(BuildPersona(Guid.NewGuid(), DateTimeOffset.UtcNow, request), cancellationToken);
        }

        var empleado = new Empleado(
            persona.Id,
            persona.Id,
            currentUserAccessor.GetRequiredUserId(),
            persona.TipoIdentificacion,
            persona.Identificacion,
            persona.RazonSocialONombresCompletos,
            persona.NombreComercial,
            persona.DireccionPrincipal,
            persona.CorreoElectronicoPrincipal,
            persona.TelefonoCelular,
            persona.FechaNacimiento,
            persona.Genero,
            NormalizeOptional(request.CodigoEmpleado),
            NormalizeOptional(request.CodigoBiometrico),
            request.FechaIngreso,
            request.FechaSalida,
            request.TipoContrato.Trim(),
            NormalizeOptional(request.CargoPuesto),
            request.SueldoBase,
            request.PorcentajeComisionVentas,
            request.EstadoLaboral.Trim(),
            NormalizeOptional(request.NombreContactoEmergencia),
            NormalizeOptional(request.TelefonoEmergencia),
            persona.RolesPersona.Concat(["Empleado"]).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            request.IsActive,
            DateTimeOffset.UtcNow,
            currentUserAccessor.GetRequiredUserId(),
            null);

        return MapToResponse(await empleadoRepository.CreateAsync(empleado, cancellationToken));
    }

    public async Task<EmpleadoResponse?> UpdateAsync(Guid id, EmpleadoRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogValuesAsync(request, cancellationToken);
        EcuadorIdentificationValidator.EnsureValid(request.TipoIdentificacion, request.Identificacion, "el empleado");

        var current = await empleadoRepository.GetByIdAsync(id, cancellationToken);
        if (current is null)
        {
            return null;
        }

        var existingPersona = await personaRepository.FindByIdentificacionAsync(request.Identificacion, cancellationToken);
        if (existingPersona is not null && existingPersona.Id != current.PersonaId)
        {
            throw new InvalidOperationException("La identificacion pertenece a otra persona. Usa esa persona para agregar el rol correspondiente.");
        }

        var currentPersona = await personaRepository.GetByIdAsync(current.PersonaId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro la persona asociada al empleado.");

        var updatedPersona = await personaRepository.UpdateAsync(BuildPersona(currentPersona.Id, currentPersona.CreatedAt, request), cancellationToken)
            ?? throw new InvalidOperationException("No se pudo actualizar la persona del empleado.");

        var empleado = new Empleado(
            id,
            current.PersonaId,
            current.EmpresaId,
            updatedPersona.TipoIdentificacion,
            updatedPersona.Identificacion,
            updatedPersona.RazonSocialONombresCompletos,
            updatedPersona.NombreComercial,
            updatedPersona.DireccionPrincipal,
            updatedPersona.CorreoElectronicoPrincipal,
            updatedPersona.TelefonoCelular,
            updatedPersona.FechaNacimiento,
            updatedPersona.Genero,
            NormalizeOptional(request.CodigoEmpleado),
            NormalizeOptional(request.CodigoBiometrico),
            request.FechaIngreso,
            request.FechaSalida,
            request.TipoContrato.Trim(),
            NormalizeOptional(request.CargoPuesto),
            request.SueldoBase,
            request.PorcentajeComisionVentas,
            request.EstadoLaboral.Trim(),
            NormalizeOptional(request.NombreContactoEmergencia),
            NormalizeOptional(request.TelefonoEmergencia),
            updatedPersona.RolesPersona,
            request.IsActive,
            current.CreatedAt,
            current.UsuarioCreacionId,
            DateTimeOffset.UtcNow);

        var updated = await empleadoRepository.UpdateAsync(empleado, cancellationToken);
        return updated is null ? null : MapToResponse(updated);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return empleadoRepository.DeleteAsync(id, cancellationToken);
    }

    private static Persona BuildPersona(Guid id, DateTimeOffset createdAt, EmpleadoRequest request)
    {
        return new Persona(
            id,
            request.TipoIdentificacion.Trim(),
            request.Identificacion.Trim(),
            request.RazonSocialONombresCompletos.Trim(),
            NormalizeOptional(request.NombreComercial),
            request.DireccionPrincipal.Trim(),
            NormalizeOptional(request.TelefonoCelular),
            NormalizeOptional(request.CorreoElectronicoPrincipal),
            request.FechaNacimiento,
            NormalizeOptional(request.Genero),
            [],
            request.IsActive,
            createdAt,
            DateTimeOffset.UtcNow);
    }

    private static EmpleadoResponse MapToResponse(Empleado empleado)
    {
        return new EmpleadoResponse
        {
            Id = empleado.Id,
            PersonaId = empleado.PersonaId,
            TipoIdentificacion = empleado.TipoIdentificacion,
            Identificacion = empleado.Identificacion,
            RazonSocialONombresCompletos = empleado.RazonSocialONombresCompletos,
            NombreComercial = empleado.NombreComercial,
            DireccionPrincipal = empleado.DireccionPrincipal,
            CorreoElectronicoPrincipal = empleado.CorreoElectronicoPrincipal,
            TelefonoCelular = empleado.TelefonoCelular,
            FechaNacimiento = empleado.FechaNacimiento,
            Genero = empleado.Genero,
            CodigoEmpleado = empleado.CodigoEmpleado,
            CodigoBiometrico = empleado.CodigoBiometrico,
            FechaIngreso = empleado.FechaIngreso,
            FechaSalida = empleado.FechaSalida,
            TipoContrato = empleado.TipoContrato,
            CargoPuesto = empleado.CargoPuesto,
            SueldoBase = empleado.SueldoBase,
            PorcentajeComisionVentas = empleado.PorcentajeComisionVentas,
            EstadoLaboral = empleado.EstadoLaboral,
            NombreContactoEmergencia = empleado.NombreContactoEmergencia,
            TelefonoEmergencia = empleado.TelefonoEmergencia,
            RolesPersona = empleado.RolesPersona,
            IsActive = empleado.IsActive,
            CreatedAt = empleado.CreatedAt,
            UsuarioCreacionId = empleado.UsuarioCreacionId,
            UpdatedAt = empleado.UpdatedAt,
            UsuarioModificacionId = empleado.UsuarioModificacionId
        };
    }

    private async Task ValidateCatalogValuesAsync(EmpleadoRequest request, CancellationToken cancellationToken)
    {
        if (!await catalogoRepository.ExistsActiveItemAsync("TIPO_IDENTIFICACION", request.TipoIdentificacion.Trim(), cancellationToken))
        {
            throw new InvalidOperationException("El tipo de identificacion del empleado no coincide con los tipos soportados por facturacion electronica.");
        }
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

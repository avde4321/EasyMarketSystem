using System.Net.Mail;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Empresa.Ports.In;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Empresa;

namespace TestDeIa.Infrastructure.Adapters.Out.Empresa;

public sealed class EmpresaSmtpConfiguracionService(
    TestDeIaDbContext db,
    ITenantContextAccessor tenant,
    IDataProtectionProvider protectionProvider) : IEmpresaSmtpConfiguracionService, IEmpresaSmtpCredentialProvider
{
    private readonly IDataProtector passwordProtector = protectionProvider.CreateProtector("EasyMarket.EmpresaSmtp.Password.v1");

    public async Task<EmpresaSmtpConfiguracionResponse?> ObtenerAsync(CancellationToken cancellationToken)
    {
        var empresaId = GetEmpresaId();
        var entity = await db.EmpresaSmtpConfiguraciones.AsNoTracking()
            .SingleOrDefaultAsync(item => item.EmpresaId == empresaId, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<EmpresaSmtpConfiguracionResponse> GuardarAsync(
        EmpresaSmtpConfiguracionRequest request,
        CancellationToken cancellationToken)
    {
        var empresaId = GetEmpresaId();
        var host = request.Host.Trim();
        var sender = request.CorreoRemitente.Trim();
        var userName = string.IsNullOrWhiteSpace(request.Usuario) ? null : request.Usuario.Trim();
        if (host.Contains(';') || host.Contains('\\') || host.Contains('/') || host.Any(char.IsControl))
            throw new InvalidOperationException("El servidor SMTP no tiene un formato válido.");

        try
        {
            _ = new MailAddress(sender);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("El correo remitente no tiene un formato válido.");
        }

        if (request.RequiereAutenticacion && string.IsNullOrWhiteSpace(userName))
            throw new InvalidOperationException("Indica el usuario SMTP o desactiva la autenticación si el servidor no la requiere.");

        var entity = await db.EmpresaSmtpConfiguraciones
            .SingleOrDefaultAsync(item => item.EmpresaId == empresaId, cancellationToken);
        var isNew = entity is null;
        entity ??= new EmpresaSmtpConfiguracionEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        entity.Host = host;
        entity.Puerto = request.Puerto;
        entity.UsarSsl = request.UsarSsl;
        entity.RequiereAutenticacion = request.RequiereAutenticacion;
        entity.Usuario = userName;
        entity.CorreoRemitente = sender;
        entity.NombreRemitente = request.NombreRemitente.Trim();
        entity.Habilitado = request.Habilitado;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        var passwordToSave = NormalizePassword(request.Contrasena);
        if (!string.IsNullOrWhiteSpace(passwordToSave))
            entity.ContrasenaProtegida = passwordProtector.Protect(passwordToSave);

        if (request.RequiereAutenticacion && string.IsNullOrWhiteSpace(entity.ContrasenaProtegida))
            throw new InvalidOperationException("Configura la contraseña SMTP antes de habilitar el envío autenticado.");

        if (isNew)
            db.EmpresaSmtpConfiguraciones.Add(entity);

        await db.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<EmpresaSmtpCredentials?> ObtenerCredencialesAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        var entity = await db.EmpresaSmtpConfiguraciones.AsNoTracking()
            .SingleOrDefaultAsync(item => item.EmpresaId == empresaId, cancellationToken);
        if (entity is null)
            return null;

        string? password = null;
        if (!string.IsNullOrWhiteSpace(entity.ContrasenaProtegida))
        {
            try
            {
                password = NormalizePassword(passwordProtector.Unprotect(entity.ContrasenaProtegida));
            }
            catch (Exception exception) when (exception is CryptographicException or FormatException)
            {
                throw new InvalidOperationException("No se pudo descifrar la contraseña SMTP. Reingrésala en la configuración de correo.", exception);
            }
        }

        return new EmpresaSmtpCredentials(
            entity.EmpresaId, entity.Host, entity.Puerto, entity.UsarSsl,
            entity.RequiereAutenticacion, entity.Usuario, password,
            entity.CorreoRemitente, entity.NombreRemitente, entity.Habilitado);
    }

    private Guid GetEmpresaId() => tenant.EmpresaId is { } id && id != Guid.Empty
        ? id
        : throw new UnauthorizedAccessException("Seleccione una empresa para configurar su correo SMTP.");

    // Google muestra las contraseñas de aplicación agrupadas visualmente; SMTP debe recibirlas sin espacios.
    private static string? NormalizePassword(string? value) => string.IsNullOrWhiteSpace(value)
        ? null
        : string.Concat(value.Where(character => !char.IsWhiteSpace(character)));

    private static EmpresaSmtpConfiguracionResponse Map(EmpresaSmtpConfiguracionEntity entity) => new()
    {
        EmpresaId = entity.EmpresaId,
        Host = entity.Host,
        Puerto = entity.Puerto,
        UsarSsl = entity.UsarSsl,
        RequiereAutenticacion = entity.RequiereAutenticacion,
        Usuario = entity.Usuario,
        ContrasenaConfigurada = !string.IsNullOrWhiteSpace(entity.ContrasenaProtegida),
        CorreoRemitente = entity.CorreoRemitente,
        NombreRemitente = entity.NombreRemitente,
        Habilitado = entity.Habilitado,
        Actualizado = entity.UpdatedAt
    };
}

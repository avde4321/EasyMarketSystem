namespace TestDeIa.Domain.Modules.Security.Entities;

public enum SecurityAuditEventType
{
    LoginExitoso = 1,
    LoginFallido = 2,
    CambioClave = 3,
    ResetClaveAdministrative = 4,
    BloqueoUsuario = 5,
    DesbloqueoUsuario = 6,
    CambioPerfil = 7,
    CambioEstado = 8,
    SuscripcionInicial = 9,
    PagoSaasRegistrado = 10,
    PagoSaasAprobado = 11,
    PagoSaasRechazado = 12,
    SuperAdminInicializado = 13,
    CuentaRetiradaParaSudo = 14
}

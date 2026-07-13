using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

internal static class CatalogSeedData
{
    internal static readonly Guid TipoIdentificacionCatalogoId = Guid.Parse("70000000-0000-0000-0000-000000000001");
    internal static readonly Guid EstadoCivilCatalogoId = Guid.Parse("70000000-0000-0000-0000-000000000002");
    internal static readonly Guid EstadoRegistroCatalogoId = Guid.Parse("70000000-0000-0000-0000-000000000003");
    internal static readonly Guid EstadoDocumentoElectronicoCatalogoId = Guid.Parse("70000000-0000-0000-0000-000000000004");
    internal static readonly Guid AmbienteSriCatalogoId = Guid.Parse("70000000-0000-0000-0000-000000000005");
    internal static readonly Guid TipoEmisionCatalogoId = Guid.Parse("70000000-0000-0000-0000-000000000006");
    internal static readonly Guid FormaPagoSriCatalogoId = Guid.Parse("70000000-0000-0000-0000-000000000007");

    internal static CatalogoEntity[] GetCatalogos()
    {
        return
        [
            new() { Id = TipoIdentificacionCatalogoId, Codigo = "TIPO_IDENTIFICACION", Nombre = "Tipo de identificacion", Descripcion = "Tipos base de identificacion para personas y clientes.", IsActive = true },
            new() { Id = EstadoCivilCatalogoId, Codigo = "ESTADO_CIVIL", Nombre = "Estado civil", Descripcion = "Estado civil de personas.", IsActive = true },
            new() { Id = EstadoRegistroCatalogoId, Codigo = "ESTADO_REGISTRO", Nombre = "Estado de registro", Descripcion = "Estados funcionales de registros activos e inactivos.", IsActive = true },
            new() { Id = EstadoDocumentoElectronicoCatalogoId, Codigo = "ESTADO_DOCUMENTO_ELECTRONICO", Nombre = "Estado documento electronico", Descripcion = "Estados de comprobantes electronicos.", IsActive = true },
            new() { Id = AmbienteSriCatalogoId, Codigo = "AMBIENTE_SRI", Nombre = "Ambiente SRI", Descripcion = "Ambiente de emision para comprobantes electronicos.", IsActive = true },
            new() { Id = TipoEmisionCatalogoId, Codigo = "TIPO_EMISION", Nombre = "Tipo de emision", Descripcion = "Tipo de emision de documentos electronicos.", IsActive = true },
            new() { Id = FormaPagoSriCatalogoId, Codigo = "FORMA_PAGO_SRI", Nombre = "Forma de pago SRI", Descripcion = "Formas de pago segun catalogo del SRI.", IsActive = true }
        ];
    }

    internal static CatalogoItemEntity[] GetCatalogoItems()
    {
        return
        [
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000001"), CatalogoId = TipoIdentificacionCatalogoId, Codigo = "04", Nombre = "RUC", Orden = 1, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000002"), CatalogoId = TipoIdentificacionCatalogoId, Codigo = "05", Nombre = "Cedula", Orden = 2, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000003"), CatalogoId = TipoIdentificacionCatalogoId, Codigo = "06", Nombre = "Pasaporte", Orden = 3, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000004"), CatalogoId = TipoIdentificacionCatalogoId, Codigo = "07", Nombre = "Consumidor final", Orden = 4, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000005"), CatalogoId = TipoIdentificacionCatalogoId, Codigo = "08", Nombre = "Identificacion del exterior", Orden = 5, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000006"), CatalogoId = TipoIdentificacionCatalogoId, Codigo = "09", Nombre = "Placa", Orden = 6, IsActive = true },

            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000011"), CatalogoId = EstadoCivilCatalogoId, Codigo = "SOLTERO", Nombre = "Soltero", Orden = 1, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000012"), CatalogoId = EstadoCivilCatalogoId, Codigo = "CASADO", Nombre = "Casado", Orden = 2, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000013"), CatalogoId = EstadoCivilCatalogoId, Codigo = "DIVORCIADO", Nombre = "Divorciado", Orden = 3, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000014"), CatalogoId = EstadoCivilCatalogoId, Codigo = "VIUDO", Nombre = "Viudo", Orden = 4, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000015"), CatalogoId = EstadoCivilCatalogoId, Codigo = "UNION_LIBRE", Nombre = "Union libre", Orden = 5, IsActive = true },

            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000021"), CatalogoId = EstadoRegistroCatalogoId, Codigo = "ACTIVO", Nombre = "Activo", Orden = 1, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000022"), CatalogoId = EstadoRegistroCatalogoId, Codigo = "INACTIVO", Nombre = "Inactivo", Orden = 2, IsActive = true },

            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000031"), CatalogoId = EstadoDocumentoElectronicoCatalogoId, Codigo = "Pendiente", Nombre = "Pendiente", Orden = 1, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000032"), CatalogoId = EstadoDocumentoElectronicoCatalogoId, Codigo = "EnProceso", Nombre = "En proceso", Orden = 2, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000033"), CatalogoId = EstadoDocumentoElectronicoCatalogoId, Codigo = "Recibido", Nombre = "Recibido", Orden = 3, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000034"), CatalogoId = EstadoDocumentoElectronicoCatalogoId, Codigo = "Autorizado", Nombre = "Autorizado", Orden = 4, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000035"), CatalogoId = EstadoDocumentoElectronicoCatalogoId, Codigo = "Rechazado", Nombre = "Rechazado", Orden = 5, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000036"), CatalogoId = EstadoDocumentoElectronicoCatalogoId, Codigo = "Error", Nombre = "Error", Orden = 6, IsActive = true },

            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000041"), CatalogoId = AmbienteSriCatalogoId, Codigo = "1", Nombre = "Pruebas", Descripcion = "Codigo SRI 1", Orden = 1, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000042"), CatalogoId = AmbienteSriCatalogoId, Codigo = "2", Nombre = "Produccion", Descripcion = "Codigo SRI 2", Orden = 2, IsActive = true },

            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000051"), CatalogoId = TipoEmisionCatalogoId, Codigo = "1", Nombre = "Normal", Descripcion = "Codigo SRI 1", Orden = 1, IsActive = true },

            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000061"), CatalogoId = FormaPagoSriCatalogoId, Codigo = "01", Nombre = "Efectivo", Descripcion = "Codigo SRI 01", Orden = 1, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000062"), CatalogoId = FormaPagoSriCatalogoId, Codigo = "15", Nombre = "Compensacion", Descripcion = "Codigo SRI 15", Orden = 2, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000063"), CatalogoId = FormaPagoSriCatalogoId, Codigo = "16", Nombre = "Tarjeta de debito", Descripcion = "Codigo SRI 16", Orden = 3, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000064"), CatalogoId = FormaPagoSriCatalogoId, Codigo = "17", Nombre = "Dinero electronico", Descripcion = "Codigo SRI 17", Orden = 4, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000065"), CatalogoId = FormaPagoSriCatalogoId, Codigo = "18", Nombre = "Tarjeta prepago", Descripcion = "Codigo SRI 18", Orden = 5, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000066"), CatalogoId = FormaPagoSriCatalogoId, Codigo = "19", Nombre = "Tarjeta de credito", Descripcion = "Codigo SRI 19", Orden = 6, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000067"), CatalogoId = FormaPagoSriCatalogoId, Codigo = "20", Nombre = "Transferencia", Descripcion = "Codigo SRI 20", Orden = 7, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000068"), CatalogoId = FormaPagoSriCatalogoId, Codigo = "21", Nombre = "Endoso de titulos", Descripcion = "Codigo SRI 21", Orden = 8, IsActive = true }
        ];
    }
}

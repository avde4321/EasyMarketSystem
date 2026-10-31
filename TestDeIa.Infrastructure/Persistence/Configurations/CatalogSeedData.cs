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
    internal static readonly Guid RetencionIvaCatalogoId = Guid.Parse("70000000-0000-0000-0000-000000000008");
    internal static readonly Guid RetencionRentaCatalogoId = Guid.Parse("70000000-0000-0000-0000-000000000009");

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
            new() { Id = FormaPagoSriCatalogoId, Codigo = "FORMA_PAGO_SRI", Nombre = "Forma de pago SRI", Descripcion = "Formas de pago segun catalogo del SRI.", IsActive = true },
            new() { Id = RetencionIvaCatalogoId, Codigo = "RETENCION_IVA_SRI", Nombre = "Retenciones IVA SRI", Descripcion = "Codigos base de retencion de IVA para documentos de proveedor.", IsActive = true },
            new() { Id = RetencionRentaCatalogoId, Codigo = "RETENCION_RENTA_SRI", Nombre = "Retenciones Renta SRI", Descripcion = "Codigos base de retencion en la fuente para documentos de proveedor.", IsActive = true }
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
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000068"), CatalogoId = FormaPagoSriCatalogoId, Codigo = "21", Nombre = "Endoso de titulos", Descripcion = "Codigo SRI 21", Orden = 8, IsActive = true },

            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000081"), CatalogoId = RetencionIvaCatalogoId, Codigo = "0", Nombre = "Sin retencion IVA", Descripcion = "0%", Orden = 1, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000082"), CatalogoId = RetencionIvaCatalogoId, Codigo = "10", Nombre = "Retencion IVA 10%", Descripcion = "10%", Orden = 2, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000083"), CatalogoId = RetencionIvaCatalogoId, Codigo = "20", Nombre = "Retencion IVA 20%", Descripcion = "20%", Orden = 3, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000084"), CatalogoId = RetencionIvaCatalogoId, Codigo = "30", Nombre = "Retencion IVA 30%", Descripcion = "30%", Orden = 4, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000085"), CatalogoId = RetencionIvaCatalogoId, Codigo = "50", Nombre = "Retencion IVA 50%", Descripcion = "50%", Orden = 5, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000086"), CatalogoId = RetencionIvaCatalogoId, Codigo = "70", Nombre = "Retencion IVA 70%", Descripcion = "70%", Orden = 6, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000087"), CatalogoId = RetencionIvaCatalogoId, Codigo = "100", Nombre = "Retencion IVA 100%", Descripcion = "100%", Orden = 7, IsActive = true },

            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000091"), CatalogoId = RetencionRentaCatalogoId, Codigo = "0", Nombre = "Sin retencion renta", Descripcion = "0%", Orden = 1, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000092"), CatalogoId = RetencionRentaCatalogoId, Codigo = "312", Nombre = "Retencion renta codigo 312", Descripcion = "1.75%", Orden = 2, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000093"), CatalogoId = RetencionRentaCatalogoId, Codigo = "320", Nombre = "Retencion renta codigo 320", Descripcion = "1.75%", Orden = 3, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000094"), CatalogoId = RetencionRentaCatalogoId, Codigo = "322", Nombre = "Retencion renta codigo 322", Descripcion = "1.75%", Orden = 4, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000095"), CatalogoId = RetencionRentaCatalogoId, Codigo = "332", Nombre = "Bienes codigo 332", Descripcion = "1.75%", Orden = 5, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000096"), CatalogoId = RetencionRentaCatalogoId, Codigo = "343", Nombre = "Servicios codigo 343", Descripcion = "2.75%", Orden = 6, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000097"), CatalogoId = RetencionRentaCatalogoId, Codigo = "344", Nombre = "Servicios codigo 344", Descripcion = "2.75%", Orden = 7, IsActive = true },
            new() { Id = Guid.Parse("71000000-0000-0000-0000-000000000098"), CatalogoId = RetencionRentaCatalogoId, Codigo = "3440", Nombre = "Retencion IVA codigo 3440", Descripcion = "70%", Orden = 8, IsActive = true }
        ];
    }
}

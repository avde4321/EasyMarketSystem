namespace TestDeIa.Shared.Security;

public static class SecurityPermissionCatalog
{
    public static readonly IReadOnlyCollection<SecurityPermissionDefinition> Definitions =
    [
        new(SecurityPermissions.DashboardView, "Acceso al dashboard principal.", "Dashboard"),
        new(SecurityPermissions.PersonasView, "Consulta y mantenimiento de personas.", "Comercial"),
        new(SecurityPermissions.ClientesView, "Consulta y mantenimiento de clientes.", "Comercial"),
        new(SecurityPermissions.EmpleadosView, "Consulta y mantenimiento de empleados.", "Comercial"),
        new(SecurityPermissions.ProveedoresView, "Consulta y mantenimiento de proveedores.", "Comercial"),
        new(SecurityPermissions.InventarioView, "Consulta de inventario y kardex.", "Inventario"),
        new(SecurityPermissions.InventarioProductos, "Creacion y actualizacion de productos.", "Inventario"),
        new(SecurityPermissions.InventarioAjustar, "Ajustes, mermas, transferencias y tomas fisicas.", "Inventario"),
        new(SecurityPermissions.InventarioBodegas, "Administracion de bodegas.", "Inventario"),
        new(SecurityPermissions.ComprasRegistrar, "Registro de compras y documentos de proveedor.", "Compras"),
        new(SecurityPermissions.ComprasLiquidaciones, "Emision y consulta de liquidaciones de compra.", "Compras"),
        new(SecurityPermissions.ComprasCuentasPorPagar, "Control de cuentas por pagar y abonos.", "Compras"),
        new(SecurityPermissions.ComprasEstudioMercado, "Analitica IA y estudio de mercado.", "Compras"),
        new(SecurityPermissions.PosFacturar, "Operacion del punto de venta y facturacion.", "Ventas"),
        new(SecurityPermissions.FacturacionMonitor, "Consulta del monitor de comprobantes.", "Ventas"),
        new(SecurityPermissions.EmpresaConfigurar, "Configuracion de empresa emisora y puntos de emision.", "Configuracion"),
        new(SecurityPermissions.CatalogosAdministrar, "Administracion de catalogos internos.", "Configuracion"),
        new(SecurityPermissions.SeguridadUsuarios, "Administracion de usuarios, roles y reseteo de claves.", "Seguridad"),
        new(SecurityPermissions.FinancieroIva, "Consulta del reporte mensual de IVA.", "Financiero"),
        new(SecurityPermissions.CajaOperar, "Apertura, cierre y control de caja.", "Caja")
    ];

    public static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> RoleMappings =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [SecurityRoleNames.Administrador] = Definitions.Select(current => current.Id).ToArray(),
            [SecurityRoleNames.Gerente] = Definitions.Select(current => current.Id).ToArray(),
            [SecurityRoleNames.Cajero] =
            [
                SecurityPermissions.DashboardView,
                SecurityPermissions.ClientesView,
                SecurityPermissions.PosFacturar,
                SecurityPermissions.FacturacionMonitor,
                SecurityPermissions.CajaOperar
            ],
            [SecurityRoleNames.AsesorComercial] =
            [
                SecurityPermissions.DashboardView,
                SecurityPermissions.ClientesView,
                SecurityPermissions.PosFacturar,
                SecurityPermissions.FacturacionMonitor
            ],
            [SecurityRoleNames.Bodeguero] =
            [
                SecurityPermissions.DashboardView,
                SecurityPermissions.InventarioView,
                SecurityPermissions.InventarioProductos,
                SecurityPermissions.InventarioAjustar,
                SecurityPermissions.InventarioBodegas
            ],
            [SecurityRoleNames.Contador] =
            [
                SecurityPermissions.DashboardView,
                SecurityPermissions.ProveedoresView,
                SecurityPermissions.ComprasRegistrar,
                SecurityPermissions.ComprasLiquidaciones,
                SecurityPermissions.ComprasCuentasPorPagar,
                SecurityPermissions.FacturacionMonitor,
                SecurityPermissions.EmpresaConfigurar,
                SecurityPermissions.FinancieroIva
            ]
        };
}

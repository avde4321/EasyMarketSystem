namespace TestDeIa.Shared.Security;

public static class SecurityPolicyCatalog
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Policies =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [SecurityPolicyNames.DashboardView] = [SecurityPermissions.DashboardView],
            [SecurityPolicyNames.ComercialAccess] =
            [
                SecurityPermissions.PersonasView,
                SecurityPermissions.ClientesView,
                SecurityPermissions.EmpleadosView,
                SecurityPermissions.ProveedoresView
            ],
            [SecurityPolicyNames.OperacionesAccess] =
            [
                SecurityPermissions.InventarioView,
                SecurityPermissions.InventarioProductos,
                SecurityPermissions.InventarioAjustar,
                SecurityPermissions.ComprasRegistrar,
                SecurityPermissions.ComprasLiquidaciones,
                SecurityPermissions.ComprasCuentasPorPagar,
                SecurityPermissions.ComprasEstudioMercado,
                SecurityPermissions.PosFacturar,
                SecurityPermissions.CajaOperar
            ],
            [SecurityPolicyNames.ConfiguracionAccess] =
            [
                SecurityPermissions.EmpresaConfigurar,
                SecurityPermissions.CatalogosAdministrar,
                SecurityPermissions.FacturacionMonitor,
                SecurityPermissions.FinancieroIva
            ],
            [SecurityPolicyNames.PersonasView] = [SecurityPermissions.PersonasView],
            [SecurityPolicyNames.ClientesView] = [SecurityPermissions.ClientesView],
            [SecurityPolicyNames.EmpleadosView] = [SecurityPermissions.EmpleadosView],
            [SecurityPolicyNames.ProveedoresView] = [SecurityPermissions.ProveedoresView],
            [SecurityPolicyNames.InventarioView] = [SecurityPermissions.InventarioView],
            [SecurityPolicyNames.InventarioManage] =
            [
                SecurityPermissions.InventarioProductos,
                SecurityPermissions.InventarioAjustar,
                SecurityPermissions.InventarioBodegas
            ],
            [SecurityPolicyNames.ComprasRegistrar] = [SecurityPermissions.ComprasRegistrar],
            [SecurityPolicyNames.ComprasLiquidaciones] = [SecurityPermissions.ComprasLiquidaciones],
            [SecurityPolicyNames.ComprasCuentasPorPagar] = [SecurityPermissions.ComprasCuentasPorPagar],
            [SecurityPolicyNames.ComprasEstudioMercado] = [SecurityPermissions.ComprasEstudioMercado],
            [SecurityPolicyNames.PosFacturar] = [SecurityPermissions.PosFacturar],
            [SecurityPolicyNames.FacturacionMonitor] = [SecurityPermissions.FacturacionMonitor],
            [SecurityPolicyNames.EmpresaConfigurar] = [SecurityPermissions.EmpresaConfigurar],
            [SecurityPolicyNames.CatalogosAdministrar] = [SecurityPermissions.CatalogosAdministrar],
            [SecurityPolicyNames.FinancieroIva] = [SecurityPermissions.FinancieroIva],
            [SecurityPolicyNames.UsuariosAdministrar] = [SecurityPermissions.SeguridadUsuarios],
            [SecurityPolicyNames.CajaOperar] = [SecurityPermissions.CajaOperar]
            ,
            [SecurityPolicyNames.ReporteriaVentas] = [SecurityPermissions.ReporteriaVentas]
        };
}

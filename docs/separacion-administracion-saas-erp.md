# Separación de administración SaaS y operación ERP

## Implementado

- Dashboard de plataforma `/saas/dashboard`: suscripciones activas, demos vigentes, vencimientos, pagos pendientes y cobros SaaS aprobados del mes (fecha de aprobación, período Ecuador UTC-5). No consulta Facturas, Personas, Clientes, Inventario ni ventas de los inquilinos.
- Acceso a solicitudes de pagos desde el panel. Tanto API como servicio verifican al superadministrador; el servicio valida la asignación activa en BD.
- Inicio y login del superadministrador llevan al panel SaaS. El resto de usuarios conserva su dashboard original.
- Dashboard ERP del propietario en `/mi-empresa/dashboard`, usando el componente gerencial original y la empresa operativa seleccionada.
- Empresas del propietario: deben tener `OwnerUserId` igual a su usuario **y** asignación explícita en `SecurityUserEmpresas`. Una asociación heredada con la demo o con un cliente no otorga acceso comercial al superadministrador.
- Resolución de empresa valida propiedad para la identidad de plataforma desde BD, incluso con JWT anteriores. El filtro global y las lecturas administrativas de empresas, certificados, puntos y configuración se endurecen con la misma regla.
- Middleware bloquea APIs ERP del propietario sin empresa operativa válida; las rutas administrativas SaaS tienen excepciones explícitas y mantienen autorización propia.
- Selector global solo lista sus empresas. Menú operativo se oculta si no hay una propia seleccionada. Se permite crear su empresa por el asistente existente.

## Conservado

- Roles, permisos y dashboards de administradores, gerentes, cajeros y bodegueros clientes.
- Reglas de suscripción, cuotas, facturación, firma SRI, inventario y caja.
- Los permisos operativos del propietario se conservan para operar **sus propias empresas**, no como acceso global a empresas clientes.
- No se borran asociaciones, empresas ni datos históricos. No se cambia esquema ni se requiere migración SQL.

## Operación

Cerrar sesión e ingresar nuevamente para actualizar el listado del JWT. La defensa backend también aplica a tokens antiguos.
Si una empresa operativa legítima no aparece, verificar `OwnerUserId` y la asignación del usuario. No reasignar empresas clientes al propietario de la plataforma para hacerlas visibles.
Los cobros SaaS aprobados no sustituyen una factura tributaria del servicio: dicha factura se emite desde la empresa operativa del proveedor, con el cliente SaaS registrado como su cliente comercial.

## Verificación

Pruebas automatizadas cubren bloqueo por empresa ajena aun con asociación, acceso a empresa propia, mantenimiento de acceso para administradores normales y bloqueo ERP sin empresa propia. No se ha realizado validación visual con navegador en esta sesión.

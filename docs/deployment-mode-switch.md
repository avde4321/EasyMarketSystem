# Modo de despliegue

La configuración del servidor `DeploymentSettings` se valida al iniciar. El valor predeterminado sigue siendo SaaS, con suscripciones y cuotas habilitadas.

Para una instalación local configure `Mode: OnPremise` y reinicie el servidor. Este modo omite las suscripciones, topes, reservas/contadores SaaS y permisos de módulos por plan. No modifica ni elimina suscripciones existentes. Blazor consulta el mismo servicio de estado del servidor, por lo que no requiere un bypass separado en el navegador.

`EnforceSubscription` y `EnforceQuotas` permiten controlar por separado suscripción y límites en SaaS. Deshabilitar cuotas no elimina la validación de suscripción ni los permisos de módulos del plan.

La empresa local se resuelve entre empresas activas. Si existe exactamente una, se utiliza automáticamente. Si hay varias, debe configurar `OnPremiseEmpresaId` con el Guid de la instalación. No se selecciona arbitrariamente una empresa. Un encabezado que solicite otra empresa se rechaza; el usuario debe seguir estando vinculado a la empresa local. Al cambiar de modalidad cierre sesión y vuelva a entrar para limpiar selecciones anteriores.

No se alteran autenticación, roles, filtros globales, claves SRI, inventario ni reglas contables. Los bloqueos transaccionales compartidos se conservan. No se necesita migración de esquema.

Al asignar una empresa existente al modo local no se genera una suscripción inicial. Este cambio no implementa un emisor/validador criptográfico de licencias autónomas: ese contrato debe definirse aparte. El onboarding público y los portales de cobro existentes son flujos SaaS; no deben publicarse como mecanismo de alta de una instalación local. Los workers de sistema conservan su alcance actual: use una base exclusiva por instalación local, no una copia multiempresa productiva con workers habilitados.

No se cambió el modo real del entorno de desarrollo; permanece SaaS para evitar desactivar sus controles accidentalmente.

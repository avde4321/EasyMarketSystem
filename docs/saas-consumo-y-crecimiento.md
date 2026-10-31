# Control de consumo y crecimiento SaaS

## Acceso y privacidad

Menú **Administración SaaS → Consumo y crecimiento**, ruta `/saas/consumo`. También enlazado desde el dashboard de plataforma.
API GET `/api/v1/saas/billing/admin/consumption?term=...&skip=0&take=20`.
Se exige rol SuperAdministrador en API y asignación activa real en BD en el servicio. El portal no requiere seleccionar una empresa ERP ni tener su suscripción activa.
Únicamente se devuelven datos identificativos de empresa, contrato y cantidades. No se incluyen identificadores de facturas, claves de acceso, clientes, líneas, XML ni valores de ventas.

## Indicadores y semántica

- Consulta actual por empresa, filtro RUC/razón social, paginación 10/20/50 y actualización manual.
- Plan y estado/vigencia actuales; no se reconstruye un contrato histórico inexistente en el esquema.
- Cupo consumido: máximo entre confirmaciones del registro de cupos del mes después del último reinicio y contador mensual vigente, igual que la validación transaccional existente.
- Reservas: todas las reservas pendientes, incluidas las arrastradas, igual que el bloqueo de cupos existente.
- Disponible: límite menos consumo menos reservas, mínimo cero; cero en el límite del plan significa ilimitado.
- Generadas: número de filas Facturas creadas en el mes hasta la consulta, independientemente de su estado SRI.
- Autorizadas: facturas actualmente AUTORIZADO con fecha de autorización en el mes, y equivalente del mes anterior completo. Los reintentos no crean conteos adicionales sobre el mismo Id.
- El mes es **UTC**, para no alterar la semántica actual de las cuotas. La UI lo advierte. No se cambió a mes Ecuador silenciosamente.
- Renovar puede reiniciar cupos sin cambiar las cantidades reales del mes. Se muestran por separado.
- Se señalan diferencias entre contador y registro de cupos, y empresas con cero/múltiples suscripciones. No se corrigen datos automáticamente; múltiples suscripciones requieren regularización antes de recomendar.
- Se informa si los topes están desactivados por configuración de despliegue. La consulta nunca activa restricciones nuevas.

## Propuesta de ampliación

Regla determinista y explicable, no IA:
1. Revisar si consumo más reservas ocupa al menos el 80% del límite o lo alcanza.
2. Desde el día 7: extrapolar autorizadas actuales / días transcurridos × días del mes; revisar si supera el límite.
3. Crecimiento estimado: proyección actual frente al total autorizado del mes anterior. Si el anterior es cero o aún no hay 7 días, no se calcula porcentaje.
4. Buscar el plan activo con la menor capacidad suficiente que aumente el cupo y cubra la proyección/consumo, conservando módulos, usuarios y sucursales, incluidos valores ilimitados. Los ilimitados se consideran al final; en empate de capacidad se usa precio y luego Id. Así un plan personalizado con precio cero no desplaza artificialmente a todos los planes.
5. Si no existe plan compatible, sugerir revisión comercial personalizada. Para suscripciones inválidas, regularizar primero; planes ilimitados no reciben propuesta por volumen.

No hay cambio automático de plan, cobro, envío comercial ni modificación de contratos. Es una ayuda de gestión, no una predicción garantizada. Las cantidades se consultan en varias lecturas breves y pueden variar con emisiones concurrentes; el control transaccional de cuotas sigue siendo la autoridad para permitir emitir.

## Rendimiento y DBA

Lecturas AsNoTracking y proyecciones SQL; primero se pagina la empresa, después se agregan cantidades de máximo 50 empresas y dos meses. No se materializan facturas completas ni se ejecuta una consulta por fila. El catálogo de planes activos se usa únicamente para recomendar.
Se reutiliza `SaasFacturaCupos (EmpresaId, Estado, PeriodoAutorizacion)`. Verificar planes de ejecución y evaluar índices `Facturas (EmpresaId, CreatedAt)` y `(EmpresaId, FechaAutorizacion) INCLUDE (Estado)` antes de añadir duplicados. AsNoTracking no implica NOLOCK: evaluar READ_COMMITTED_SNAPSHOT con el DBA si las lecturas sufren bloqueos.
No se añadieron tablas, migraciones ni cambios destructivos. No se modificaron firma SRI, emisión, reglas de autorización, renovación, cupos ni dashboards de las empresas.

## Validación

Pruebas de umbral 80%, reservas, límite, proyección temprana/madura, crecimiento sin base, ilimitados y protección de módulos/capacidades.
Prueba SQL Server de agregados, reinicio de cuota frente a volumen, privacidad de respuesta, paginación, acceso restringido y ausencia de cambios en contratos/contadores.
La revisión visual en navegador queda pendiente; no se ha iniciado la API con workers de emisión para esta validación.

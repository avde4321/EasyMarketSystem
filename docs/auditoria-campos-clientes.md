# Auditoría del formulario y flujo de clientes

## Matriz de campos

| Campo | Persona natural | Empresa / jurídica local | Regla y corrección |
|---|---|---|---|
| Tipo de cliente | Natural | Jurídico | Selección explícita. Un RUC no basta para inferir personalidad jurídica. |
| Identificación | Cédula, RUC u otros tipos soportados | RUC en el flujo doméstico actual | Cédula 10 dígitos, RUC 13. Formato en DTO, validación ecuatoriana existente en backend. No se cambiaron algoritmos globales. |
| Consumidor final | Caso genérico separado | No aplica | Código 07, identificación 9999999999999 y denominación canónica. UI fija número/nombre; sin género, nacimiento, campos tributarios ni línea de crédito individual. Sigue aplicando unicidad por empresa existente. |
| Nombres / razón social | Nombres y apellidos completos | Razón social | Obligatorio, máximo 180. Se conserva texto completo como dato maestro aprobado; no se exige segundo nombre ni se inventan apellidos. |
| Nombre comercial | Opcional al registrar con RUC | Opcional | No se presenta como obligatorio para todas las personas naturales. Datos históricos no se borran masivamente. Un cambio explícito a un tipo sin campos comerciales limpia el valor del formulario. |
| Correo principal | Opcional | Opcional | Formato email, máximo 180. Contacto general de Persona. |
| Correo para comprobantes | Opcional | Opcional | Dato del rol Cliente, accesible también al reutilizar Persona. Facturación y búsqueda POS priorizan este correo; si no existe usan el principal. No cambia el correo maestro de Persona. |
| Teléfono | Opcional | Opcional | Se rotula contacto, no exclusivamente celular. Máximo 40, input tel y validación Phone. Se aceptan formatos internacionales y vacío; no se impone una máscara local. |
| Género | Opcional | No aplica | Se oculta y no se persiste para empresas/consumidor final. No es requisito de facturación. |
| Nacimiento | Opcional | No aplica | Se impide fecha futura en validación compartida/backend. Se oculta para empresa/consumidor final. |
| Región/provincia/ciudad/sector | Opcionales | Opcionales | Se mantiene el selector jerárquico existente; no se agregaron referencias nuevas. |
| Dirección exacta | Obligatoria según contrato existente | Obligatoria | Máximo 250. No se inventa dirección de una persona existente al agregar rol. |
| Obligado a contabilidad | Cuando se registra con RUC | Cuando corresponda según registro | No se infiere solo del tipo de cliente. Campo tributario requiere RUC, no se presenta para cédula/pasaporte/consumidor final. |
| Contribuyente especial | Cuando corresponda con RUC | Cuando corresponda con RUC | Misma regla de visualización/validación. Sigue siendo indicador booleano existente, no una consulta/certificación del catastro SRI. |
| Permite crédito | Política comercial | Política comercial | Disponible para cliente identificado, no consumidor final. No se confunde con condición tributaria. |
| Límite de crédito | Si crédito habilitado | Si crédito habilitado | No negativo, límite técnico existente 999999999. UI muestra error y deshabilita edición al no permitir crédito; no elimina automáticamente condiciones guardadas. |
| Días de crédito | Si crédito habilitado | Si crédito habilitado | Rango existente 0–3650. No se inventó que cero signifique ilimitado o se impuso plazo mínimo. |
| Estado de crédito | Al día / Suspendido / Bloqueado | Igual | Se unificó contrato con UI. Valor heredado Normal se interpreta como AlDia. Valores arbitrarios se rechazan. |
| Cliente activo | Sí | Sí | Solo afecta rol Cliente; al agregar rol no cambia silenciosamente estado/datos de la Persona. |

## Correcciones del flujo

- Agregar Cliente a Persona existente ya no sobrescribe sus datos maestros con un formulario oculto/desactualizado. La edición explícita mantiene el flujo anterior de actualización.
- Selección/búsqueda de otra Persona reinicia los datos comerciales del borrador para no arrastrar correo o condiciones de crédito del candidato anterior.
- La búsqueda por identificación reutiliza el mismo método de selección que la consulta principal.
- El caso de uso valida también los atributos del DTO antes de persistir, no únicamente cuando se llama mediante MVC. Errores de campos no crean primero una Persona.
- Se corrigió el identificador de empresa del objeto de dominio: se obtiene del contexto de empresa, no del identificador del usuario. El repositorio ya imponía el tenant correcto y se mantuvo esa defensa.
- Se muestran errores dentro del popup y se interpretan respuestas ValidationProblemDetails/ProblemDetails del API en lugar de un error genérico. Se agrega resumen de validación y se bloquea cerrar durante guardado.

## Conservado y límites de esta revisión

- Sin nuevas tablas/migraciones ni modificación masiva de datos del usuario. Se mantienen Persona, Cliente, PersonasNaturales, EmpresasCliente, sus índices y aislamiento multiempresa.
- La clasificación natural/jurídica y los datos tributarios son declarados: no se implementó validación automática contra el catastro SRI.
- El modelo actual para empresa cliente es doméstico (RUC). Empresas extranjeras con identificación exterior necesitan una decisión explícita; no se forzó una regla nueva ni se consideraron cubiertas por la etiqueta genérica.
- El selector geográfico conserva su filtrado visual; esta intervención no certifica que el backend compruebe todas las relaciones entre códigos enviados manualmente.
- Las condiciones de crédito son configuración. No se certifica aquí un motor de autorización financiera por saldo CXC: no se añadieron bloqueos de ventas por cupo ni se decidió el significado financiero de límite cero. Es un flujo que requiere auditoría separada.
- Los cambios de clasificación al editar pueden impactar los registros especializados mediante el repositorio existente. No se reclasificaron registros históricos automáticamente.

## Verificación

30 pruebas específicas: clasificación, reutilización sin sobrescritura, fechas, correos opcionales y preferencia de facturación, teléfonos, estados/rangos de crédito, consumidor final y manejo de errores HTTP. Los escenarios del caso de uso usan repositorios reales con EF InMemory y no sustituyen pruebas de restricciones/concurrencia SQL. Se ejecutó además la suite completa. La comprobación visual con API ficticia y sin escrituras se intentó, pero Computer Use no pudo identificar con seguridad la URL del navegador y detuvo la interacción; no se declara validación visual terminada. Los servidores de QA se detuvieron.

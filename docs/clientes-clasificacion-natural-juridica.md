# Clasificación en creación directa de clientes

## Cambios
- Selector inicial Persona natural / Empresa-persona jurídica en Clientes, respetando modal y estilos existentes.
- Etiqueta dinámica de nombres completos o razón social. Género y nacimiento solo para personas naturales; se limpian al cambiar a empresa.
- Empresa utiliza RUC; una persona natural puede utilizar RUC sin convertirse automáticamente en jurídica.
- Reutilizar una Persona carga su EsPersonaJuridica y evita enviar por defecto Natural. El backend rechaza discrepancias de clasificación al agregar el rol Cliente.
- Alta y edición sincronizan TipoCliente, Persona.EsPersonaJuridica y Persona.EsEmpresa, reutilizando PersonasNaturales / EmpresasCliente mediante el repositorio existente.
- Validaciones compartidas de clasificación en DTO y comprobación en caso de uso para llamadas que no pasan por MVC.
- El aviso del identificador de consumidor final solo aparece al seleccionar su tipo.

## Compatibilidad
Sin nuevas entidades ni migraciones. No se modificaron registros históricos en masa. Los datos antiguos inconsistentes deben revisarse y corregirse individualmente desde edición; no se infiere automáticamente la personalidad jurídica de todos los RUC. No se alteró el flujo de Proveedores ni de alta rápida del POS en esta intervención.

## Verificación
Diez pruebas de creación, edición y reutilización, con repositorios reales sobre EF InMemory: natural con cédula, natural con RUC, empresa, consumidor final, tipos incompatibles, duplicación de Persona y sincronización de registros especializados. InMemory no sustituye una prueba de restricciones/concurrencia en SQL Server. Compilación completa y suite de pruebas ejecutadas; no se realizaron altas manuales en la base del usuario.

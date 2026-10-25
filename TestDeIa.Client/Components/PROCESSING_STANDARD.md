# Indicador de procesos

Usar `ProcessingOverlay` para operaciones transaccionales que requieren espera: guardar,
emitir, cerrar caja, importar, convertir, conciliar, firmar y enviar documentos.

```razor
<ProcessingOverlay IsVisible="@isSaving" Title="Guardando empresa"
                   Message="Espera hasta que termine el registro." />
```

La operación debe comprobar su indicador al entrar para evitar duplicados,
activarlo antes del primer `await` y desactivarlo dentro de `finally`.
Mostrar el resultado o error después de finalizar. No mostrar porcentajes ficticios.
El componente se controla por el estado de la operación, no por un temporizador.

Las búsquedas, paginación y refrescos automáticos usan indicadores locales;
no bloquear la pantalla completa para esos procesos ni para el polling de un QR ya visible.
El indicador no sustituye validaciones, idempotencia ni concurrencia del backend.

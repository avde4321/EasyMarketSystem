# EasyMarketSystem

CRM / ERP comercial sobre .NET 9, Blazor WebAssembly y SQL Server. La solucion esta pensada para operar ventas, compras, inventario, facturacion electronica, contabilidad NIIF, activos fijos y reporterias, manteniendo separacion por empresa mediante `EmpresaId`.

## Vision general

EasyMarketSystem centraliza la operacion comercial y administrativa de una empresa con enfoque modular:

- seguridad, usuarios, roles, perfiles y auditoria
- gestion de personas, clientes, empleados y proveedores
- inventario multi-bodega con Kardex, entradas, salidas, mermas y toma fisica
- POS con control de caja, punto de emision, facturacion electronica y RIDE
- compras, liquidaciones, cuentas por pagar y bancarizacion LRTI
- comisiones por servicios
- contabilidad NIIF con plan de cuentas, partida doble, libros oficiales y estados financieros
- activos fijos con hoja de vida y parametros SRI/LRTI
- configuracion de empresa emisora, ambiente SRI y certificados
- inicializacion automatica de base de datos y datos demo

El frontend mantiene una linea visual minimalista en tonos pastel, con modales/popup para operaciones, notificaciones y errores.

## Stack tecnologico

- .NET 9
- ASP.NET Core Web API
- Blazor WebAssembly
- Entity Framework Core
- SQL Server
- JWT para autenticacion
- Background workers para procesos asincronos
- Arquitectura limpia con separacion por capas

## Estructura de la solucion

```text
TestDeIa.Api             API Web, controllers, middleware y composicion de servicios
TestDeIa.Application     Casos de uso, puertos, reglas de aplicacion y servicios
TestDeIa.Client          Frontend Blazor WebAssembly
TestDeIa.Domain          Entidades, enums y reglas de negocio
TestDeIa.Infrastructure  EF Core, persistencia, adaptadores, SRI y background workers
TestDeIa.Shared          DTOs, requests, responses, catalogos y constantes compartidas
```

## Arquitectura

La solucion usa Clean Architecture con enfoque hexagonal:

- `Domain`: nucleo del negocio, entidades y reglas puras.
- `Application`: casos de uso, comandos, servicios y contratos.
- `Infrastructure`: implementaciones EF Core, repositorios, integraciones y workers.
- `Api`: endpoints HTTP, seguridad, middleware y arranque.
- `Client`: experiencia Blazor, pantallas, layouts y clientes HTTP.
- `Shared`: contratos comunes entre API y cliente.

El `Program.cs` de API y Cliente se mantiene liviano. La composicion de servicios se orquesta en clases dedicadas:

- `TestDeIa.Api/Configuration/ApiServiceRegistration.cs`
- `TestDeIa.Api/Configuration/DatabaseStartupOrchestrator.cs`
- `TestDeIa.Client/Configuration/ClientServiceRegistration.cs`

Documento complementario:

- [ARCHITECTURE.md](./ARCHITECTURE.md)

## Multi-tenant por EmpresaId

La solucion trabaja con aislamiento por empresa activa:

- la empresa se selecciona desde la barra superior
- el cliente envia el contexto de empresa al API
- EF Core aplica filtros por `EmpresaId`
- usuarios pueden tener acceso a una o varias empresas
- el plan contable, inventario, compras, facturas y reportes se consultan dentro del tenant activo

## Inicializacion automatica de base de datos

Al iniciar la API se ejecuta `DbInitializer`:

- aplica migraciones con `Database.MigrateAsync()`
- crea o actualiza empresa demo
- asegura usuario administrador inicial
- vincula usuario, empresa, rol y punto de emision
- crea bodega principal y punto de emision demo si faltan
- precarga plan de cuentas NIIF base por empresa
- asegura catalogos base de retenciones SRI

Usuario inicial recomendado para ambiente nuevo:

```text
Email: admin@easymarket.com
Password: Admin1234!*
```

> Nota: si ya existe un usuario `admin` historico, el inicializador intenta reutilizarlo y actualizarlo de forma compatible para evitar duplicados.

## Modulos implementados

### Seguridad, roles y auditoria

- login con JWT
- validacion de token contra estado actual del usuario
- bloqueo manual o temporal
- invalidacion de sesiones
- roles: Administrador, Cajero, Bodeguero, Contador
- permisos por modulo
- administracion de usuarios
- asignacion de empresas y puntos de emision
- auditoria de seguridad

### Personas, clientes, empleados y proveedores

- catalogo central de personas
- extension de persona a cliente, empleado o proveedor
- validaciones por identificacion
- datos de contacto, direccion y estado
- proveedores con parametros tributarios por defecto
- empleados activos para operaciones y servicios

### Empresa emisora y parametros SRI

- razon social, nombre comercial, RUC y direcciones
- ambiente SRI: Pruebas `1` y Produccion `2`
- badge visible en la barra superior indicando ambiente activo
- establecimiento y punto de emision
- configuracion de certificado `.p12`
- modo desarrollo interno para simulacion
- tipo de emision SRI
- puntos de emision vinculados a bodegas

### SRI, clave de acceso y endpoints

- `ClaveAccesoService` centraliza la clave de acceso de 49 digitos
- algoritmo modulo 11 del SRI
- la posicion de ambiente se genera estrictamente desde el ambiente activo:
  - `1`: Pruebas
  - `2`: Produccion
- `SriUrlResolverService` resuelve URLs de recepcion y autorizacion por ambiente
- soporte de XML firmado, autorizacion y monitor de comprobantes
- simulacion interna cuando la empresa esta en modo desarrollo

### Inventario y Kardex

- catalogo de productos y servicios
- control de stock por producto
- multi-bodega
- costo promedio
- stock minimo
- Kardex por producto y bodega
- entradas por compra
- salidas por venta
- ajustes manuales
- mermas / bajas
- toma fisica de inventario
- grilla de Kardex con factura o comprobante asociado al ingreso/salida

### POS y facturacion electronica

- POS operativo con seleccion de punto de venta
- bloqueo si no hay caja activa o punto de emision
- busqueda de clientes y productos
- soporte de bienes y servicios
- asignacion de operador para servicios
- calculo de comisiones por servicios
- emision de factura y cola asincrona SRI
- popup informativo al generar factura/RIDE
- bloqueo del boton de cobro para evitar doble click
- descuento automatico de inventario para bienes fisicos
- RIDE PDF y XML desde monitor

### Caja

- apertura y control de caja
- arqueo y cierre diario
- desglose de efectivo, tarjeta y transferencia
- calculo de diferencia
- bloqueo de sesion cerrada
- asiento contable automatico por cierre de caja

### Compras y liquidaciones

- registro de compras de proveedor
- liquidaciones de compra
- clave de acceso proveedor y autorizacion
- sustento tributario SRI
- naturaleza de compra:
  - mercaderia / inventario
  - activo fijo
  - gasto / servicio
- impacto en Kardex solo para mercaderia
- generacion de activos fijos desde compras de activo fijo
- UI por popup/carrusel para formularios y detalle

### Bancarizacion LRTI y cuentas por pagar

- forma de pago de compra:
  - contado efectivo
  - transferencia bancaria
  - cheque
  - tarjeta de credito
  - credito proveedores
- regla LRTI: compras iguales o mayores a USD 1,000 no permiten contado efectivo
- cuentas por pagar a proveedores
- abonos parciales o pagos totales
- cuenta contable de salida de caja/banco
- comprobante o transferencia de pago
- asiento automatico de pago CxP

### Reporteria de compras

- reporte consolidado fisico y financiero de compras
- filtros por rango de fechas y naturaleza
- tab de movimiento fisico:
  - entradas de inventario
  - activos dados de alta
  - comprobante SRI asociado
- tab de flujo monetario y pasivos:
  - bases IVA 0% y 15%
  - IVA compras
  - total comprado
  - desembolsos caja/banco
  - saldos pendientes CxP

### Contabilidad NIIF

- plan de cuentas jerarquico por empresa
- cuentas aceptables y de agrupacion
- unique index por `EmpresaId + Codigo`
- periodos contables por mes/anio
- bloqueo de periodo cerrado
- motor de partida doble
- asiento contable y detalle
- validacion Debe = Haber
- excepcion de asiento descuadrado
- actualizacion de saldos contables
- asiento manual desde UI
- libro diario
- libro mayor
- balance general
- estado de resultados
- cierre de periodo fiscal con auditoria
- ajuste contable de inventario por merma/deterioro

### Activos fijos

- registro de activos fijos por empresa
- hoja de vida del activo
- categoria SRI/LRTI
- vida util y porcentaje de depreciacion automatico
- ubicacion fisica
- custodio responsable
- estados del activo
- origen desde compra de activo fijo
- no afecta Kardex de mercaderia

### Comisiones por servicios

- configuracion de comision en productos tipo servicio
- porcentaje o valor fijo
- asignacion de operador en POS
- calculo atomico al facturar
- liquidacion de comisiones por rango de fechas y empleado

### Dashboard y reportes financieros

- resumen operativo
- ventas y compras
- consumo historico
- ranking de productos/servicios
- IVA mensual
- reportes de comprobantes electronicos
- libros y estados financieros NIIF

## Experiencia de usuario

- sidebar reorganizado por bloques:
  - Comercial
  - Operaciones
  - Compras
  - Contabilidad y Finanzas
  - Configuracion
- autoexpansion de menus por ruta
- popups estandarizados para informacion, exito y errores
- formularios complejos en modales/popup
- diseno minimalista pastel
- soporte responsive y menu hamburguesa

## Configuracion local

Los valores sensibles no deben versionarse. Usar:

- `appsettings.Development.json`
- secretos locales
- variables de entorno

Valores sensibles habituales:

- cadena de conexion SQL Server
- secreto JWT
- URLs o credenciales externas
- certificado `.p12`
- password de firma electronica

## Ejecucion local

### Requisitos

- .NET SDK 9
- SQL Server disponible
- Visual Studio 2022 o CLI de .NET

### Pasos generales

1. configurar la cadena de conexion en el API
2. restaurar paquetes
3. iniciar la API
4. iniciar el cliente Blazor

La API aplica migraciones automaticamente al arrancar. Si se desea ejecutar manualmente:

```bash
dotnet restore
dotnet ef database update --project TestDeIa.Infrastructure --startup-project TestDeIa.Api
dotnet build TestDeIa.sln
```

## Validacion de build

Comando recomendado antes de cerrar cambios:

```bash
dotnet clean TestDeIa.sln
dotnet build TestDeIa.sln
```

El objetivo de la rama `DeveloperIA` es mantener:

```text
0 errores
0 warnings
```

## Rama de trabajo

- `main`: rama estable / objetivo final
- `DeveloperIA`: rama activa de evolucion

## Estado actual

El sistema cuenta con una base ERP/CRM funcional y modular, con flujos operativos de venta, compra, inventario, caja, contabilidad y SRI. La arquitectura permite seguir sumando etapas sin rehacer el nucleo, manteniendo aislamiento por empresa y responsabilidades separadas por capa.

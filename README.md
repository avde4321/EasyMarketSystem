# EasyMarketSystem

CRM / ERP comercial construido sobre ecosistema Microsoft con backend en .NET 9, frontend Blazor WebAssembly y SQL Server, pensado para crecer por modulos bajo una base de Clean Architecture con enfoque hexagonal.

## Vision general

El proyecto esta orientado a centralizar operaciones comerciales y administrativas de una empresa, con especial cuidado en:

- seguridad y autenticacion con JWT
- gestion de personas, clientes y relacion entre entidades
- inventario con Kardex y control de stock
- facturacion electronica con flujo asincrono
- configuracion de empresa emisora para cumplimiento tributario

El frontend sigue un estilo de trabajo modular con componentes Razor separados en `.razor`, `.razor.cs` y `.razor.css`.

## Stack tecnologico

- .NET 9
- ASP.NET Core Web API
- Blazor WebAssembly
- Entity Framework Core
- SQL Server
- JWT para autenticacion

## Estructura de la solucion

```text
TestDeIa.Api             API clasica con Controllers
TestDeIa.Application     Casos de uso, puertos de entrada y salida
TestDeIa.Client          Frontend Blazor WebAssembly
TestDeIa.Domain          Entidades y reglas de negocio
TestDeIa.Infrastructure  Persistencia, EF Core, adaptadores y background workers
TestDeIa.Shared          Contratos compartidos entre API y cliente
```

## Arquitectura

La solucion usa Clean Architecture con ideas de arquitectura hexagonal:

- `Domain`: contiene el nucleo del negocio
- `Application`: define casos de uso y contratos
- `Infrastructure`: implementa persistencia y adaptadores externos
- `Api`: expone endpoints HTTP
- `Client`: consume la API y presenta la experiencia de usuario

Documento de referencia interna:

- [ARCHITECTURE.md](./ARCHITECTURE.md)

## Modulos implementados

### Seguridad

- login con JWT
- control de sesion desde cliente
- expiracion de token
- base para autorizacion de pantallas

### Personas

- catalogo central de personas
- tipo de identificacion, datos de contacto y fecha de nacimiento
- exclusion de registros internos del sistema

### Clientes

- relacion con `Persona`
- validaciones por tipo de identificacion
- sincronizacion de datos personales base

### Empresa Emisora

- razon social, nombre comercial, RUC y direcciones
- ambiente SRI de pruebas o produccion
- establecimiento y punto de emision
- certificado `.p12` almacenado en base de datos

### Inventario

- catalogo de productos
- PVP, costo promedio, stock minimo y stock actual
- historial Kardex
- ajustes manuales de stock
- preparacion para descuento automatico por facturacion

### Facturacion Electronica

- punto de venta
- monitor de comprobantes
- emision asincrona
- persistencia local en estado pendiente
- procesamiento en segundo plano
- armado de XML con base en criterios del SRI ya incorporados en el flujo actual

## Flujo funcional destacado

1. un usuario inicia sesion
2. administra personas, clientes y configuracion de empresa
3. registra productos y mantiene inventario
4. emite facturas desde POS
5. la factura se guarda localmente
6. un worker asincrono procesa el comprobante
7. al autorizarse, se actualiza estado y se descuenta inventario

## Configuracion local

El repositorio deja configuraciones publicables seguras en `appsettings.json`.

Los valores sensibles deben mantenerse fuera del repositorio, por ejemplo en:

- `appsettings.Development.json`
- secretos locales
- variables de entorno

Ejemplos de datos sensibles:

- cadena de conexion SQL Server
- secreto JWT real
- certificado y clave de firma

## Ejecucion local

### Requisitos

- .NET SDK 9
- SQL Server disponible
- Visual Studio 2022 o CLI de .NET

### Pasos generales

1. configurar la cadena de conexion local del API
2. restaurar paquetes
3. aplicar migraciones
4. iniciar API
5. iniciar cliente Blazor

Comandos de referencia:

```bash
dotnet restore
dotnet ef database update --project TestDeIa.Infrastructure --startup-project TestDeIa.Api
dotnet build TestDeIa.sln
```

## Estado actual del proyecto

El sistema ya cuenta con base funcional para seguir creciendo por modulos. El enfoque actual prioriza:

- consistencia de datos entre modulos
- cumplimiento tributario progresivo en facturacion
- separacion limpia de responsabilidades
- posibilidad de escalar el desarrollo sin rehacer la base

## Rama de trabajo

- `main`: rama estable / objetivo final
- `DeveloperIA`: rama de trabajo activa para evolucion del proyecto

## Notas

Este proyecto ha sido trabajado con una estrategia incremental, consolidando primero la base arquitectonica y luego los modulos operativos principales.

# Arquitectura del Proyecto

Este proyecto usa una base de Clean Architecture con ideas de arquitectura hexagonal para permitir crecimiento por modulos.

## Capas

- `TestDeIa.Domain`: reglas del negocio, entidades, value objects y eventos de dominio.
- `TestDeIa.Application`: casos de uso, puertos de entrada, puertos de salida e interfaces.
- `TestDeIa.Infrastructure`: adaptadores externos, persistencia, repositorios, servicios externos y configuraciones.
- `TestDeIa.Api`: entrada HTTP del backend.
- `TestDeIa.Client`: frontend Blazor/Razor.
- `TestDeIa.Shared`: contratos compartidos entre API y frontend.

## Direccion de Dependencias

```text
Api -> Application, Infrastructure, Shared
Application -> Domain, Shared
Infrastructure -> Application, Domain
Client -> Shared
Domain -> nada
Shared -> nada
```

## Estructura de Modulos

Cada modulo debe agrupar su propio negocio sin depender directamente de otros modulos.

Ejemplo para un modulo `Usuarios`:

```text
TestDeIa.Domain
+-- Modules
    +-- Usuarios
        +-- Entities
        +-- ValueObjects
        +-- Events
        +-- Rules

TestDeIa.Application
+-- Modules
    +-- Usuarios
        +-- Ports
        |   +-- In
        |   +-- Out
        +-- UseCases
        +-- DTOs

TestDeIa.Infrastructure
+-- Adapters
    +-- In
    +-- Out
```

## Regla Hexagonal

La aplicacion define puertos. La infraestructura implementa adaptadores.

- Puerto de entrada: contrato que describe una accion disponible del sistema.
- Caso de uso: implementa la accion.
- Puerto de salida: contrato que necesita el caso de uso para hablar con base de datos, APIs externas u otros recursos.
- Adaptador de salida: implementacion real del puerto usando Entity Framework, HTTP, archivos, etc.

## Regla Practica

El dominio no debe saber que existe API, Blazor, Entity Framework, SQL Server, HTTP ni archivos.

Si una clase de negocio necesita algo externo, primero define un puerto en `Application` y luego implementa el adaptador en `Infrastructure`.

## Modulo Seguridad

El primer modulo implementado es `Security`.

- `Domain`: contiene la entidad `SecurityUser`.
- `Application`: contiene el caso de uso `LoginUseCase`, el puerto de entrada `ILoginUseCase` y los puertos de salida para usuarios, hashing y generacion de tokens.
- `Infrastructure`: contiene adaptadores para SQL Server con Entity Framework Core, hashing SHA256 y generacion de JWT.
- `Api`: expone `POST /api/security/login` mediante controladores MVC clasicos.
- `Client`: contiene la pantalla `/login`, cierre de sesion y almacenamiento del token en `localStorage`.

Usuario temporal de desarrollo:

```text
Usuario: admin
Contrasena: Admin123*
```

Este usuario se crea como dato inicial mediante migraciones de Entity Framework Core.

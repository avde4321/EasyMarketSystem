using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Clientes.UseCases;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Infrastructure.Adapters.Out.Catalogos;
using TestDeIa.Infrastructure.Adapters.Out.Clientes;
using TestDeIa.Infrastructure.Adapters.Out.Personas;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence.Tenancy;
using TestDeIa.Shared.Requests.Clientes;

namespace TestDeIa.Tests.Clientes;

public sealed class ClienteClassificationTests
{
    [Theory]
    [InlineData("+593 99 123 4567")]
    [InlineData("+1 (212) 555-0123")]
    [InlineData("")]
    public async Task ContactPhoneAcceptsInternationalFormatsAndCanBeEmpty(string phone)
    {
        await using var fixture = await Fixture.CreateAsync();
        var request = Request("Natural", "05", "0923902225");
        request.TelefonoCelular = phone;
        var result = await fixture.Service.CreateAsync(request);
        Assert.Equal(string.IsNullOrEmpty(phone) ? null : phone, result.TelefonoCelular);
    }

    [Theory]
    [InlineData("Natural", "05", "0923902225", false)]
    [InlineData("Natural", "04", "0923902225001", false)]
    [InlineData("Juridico", "04", "1792040531001", true)]
    [InlineData("Natural", "07", "9999999999999", false)]
    public async Task DirectCreationPersistsCorrectPersonAndSpecializedRecord(string type, string idType, string id, bool juridico)
    {
        await using var fixture = await Fixture.CreateAsync();
        var request = Request(type, idType, id);
        request.FechaNacimiento = new DateOnly(1990, 1, 1);
        request.Genero = "Prueba";
        var result = await fixture.Service.CreateAsync(request);
        var person = await fixture.Db.Personas.SingleAsync(x => x.Id == result.PersonaId);
        Assert.Equal(juridico, person.EsPersonaJuridica);
        Assert.Equal(juridico, person.EsEmpresa);
        Assert.Equal(type, result.TipoCliente);
        Assert.Equal(!juridico, await fixture.Db.PersonasNaturales.AnyAsync(x => x.Id == person.Id));
        Assert.Equal(juridico, await fixture.Db.EmpresasCliente.AnyAsync(x => x.Id == person.Id));
        if (juridico) { Assert.Null(person.FechaNacimiento); Assert.Null(person.Genero); }
    }

    [Theory]
    [InlineData("Juridico", "05")]
    [InlineData("Juridico", "07")]
    [InlineData("Otro", "04")]
    public async Task InvalidClassificationIsRejectedBeforePersistence(string type, string idType)
    {
        await using var fixture = await Fixture.CreateAsync();
        var request = Request(type, idType, "0923902225");
        var errors = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(request, new ValidationContext(request), errors, true));
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.CreateAsync(request));
        Assert.Empty(await fixture.Db.Personas.ToListAsync());
        Assert.Empty(await fixture.Db.Clientes.ToListAsync());
    }

    [Fact]
    public async Task ExistingPersonClassificationCannotBeChangedWhileAddingClientRole()
    {
        await using var fixture = await Fixture.CreateAsync();
        var id = Guid.NewGuid();
        fixture.Db.Personas.Add(new PersonaEntity { Id = id, EmpresaId = fixture.Tenant.EmpresaId!.Value,
            TipoIdentificacion = "04", Identificacion = "0923902225001", RazonSocialONombresCompletos = "Persona natural",
            DireccionPrincipal = "Dirección", IsActive = true });
        await fixture.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.CreateAsync(Request("Juridico", "04", "0923902225001")));
        Assert.False((await fixture.Db.Personas.SingleAsync()).EsPersonaJuridica);
        Assert.Empty(await fixture.Db.Clientes.ToListAsync());
        var result = await fixture.Service.CreateAsync(Request("Natural", "04", "0923902225001"));
        Assert.Equal(id, result.PersonaId);
        Assert.Equal("Persona natural", result.RazonSocialONombresCompletos);
        Assert.Equal("Dirección", result.DireccionPrincipal);
        Assert.Single(await fixture.Db.Personas.ToListAsync());
    }

    [Fact]
    public async Task EditingClientKeepsClassificationAndPersonalFieldsConsistent()
    {
        await using var fixture = await Fixture.CreateAsync();
        var request = Request("Juridico", "04", "1792040531001");
        var created = await fixture.Service.CreateAsync(request);
        request.FechaNacimiento = new DateOnly(1980, 2, 3);
        request.Genero = "No aplica";
        var updated = await fixture.Service.UpdateAsync(created.Id, request);
        Assert.NotNull(updated);
        Assert.Null(updated.FechaNacimiento);
        Assert.Null(updated.Genero);
        Assert.True((await fixture.Db.Personas.SingleAsync()).EsPersonaJuridica);
    }

    private static ClienteRequest Request(string type, string idType, string id) => new()
    {
        TipoCliente = type, TipoIdentificacion = idType, Identificacion = id,
        RazonSocialONombresCompletos = "Cliente de prueba", DireccionPrincipal = "Dirección de prueba"
    };

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task OptionalEmailsCanBeClearedWithoutBecomingInvalid(string empty)
    {
        await using var fixture = await Fixture.CreateAsync();
        var request = Request("Natural", "05", "0923902225");
        request.CorreoElectronicoPrincipal = empty;
        request.CorreoFacturacionElectronica = empty;
        var result = await fixture.Service.CreateAsync(request);
        Assert.Null(result.CorreoElectronicoPrincipal);
        Assert.Null(result.CorreoFacturacionElectronica);
    }

    [Theory]
    [InlineData("NacimientoFuturo")]
    [InlineData("TributarioSinRuc")]
    [InlineData("EstadoCreditoInvalido")]
    [InlineData("CreditoNegativo")]
    [InlineData("CorreoInvalido")]
    [InlineData("TelefonoInvalido")]
    [InlineData("ConsumidorConCredito")]
    public async Task FieldRulesRejectInvalidDataBeforeCreatingPerson(string scenario)
    {
        await using var fixture = await Fixture.CreateAsync();
        var request = Request("Natural", "05", "0923902225");
        switch (scenario)
        {
            case "NacimientoFuturo": request.FechaNacimiento = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)); break;
            case "TributarioSinRuc": request.ObligadoContabilidad = true; break;
            case "EstadoCreditoInvalido": request.EstadoCredito = "Inventado"; break;
            case "CreditoNegativo": request.LimiteCredito = -1; break;
            case "CorreoInvalido": request.CorreoFacturacionElectronica = "no-es-correo"; break;
            case "TelefonoInvalido": request.TelefonoCelular = "no-es-telefono"; break;
            case "ConsumidorConCredito": request.TipoIdentificacion = "07"; request.Identificacion = "9999999999999"; request.PermiteCredito = true; break;
        }
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.CreateAsync(request));
        Assert.Empty(await fixture.Db.Personas.ToListAsync());
        Assert.Empty(await fixture.Db.Clientes.ToListAsync());
    }

    [Fact]
    public async Task LegacyCreditStateIsNormalizedAndTermsAreNotDeletedWhenCreditIsDisabled()
    {
        await using var fixture = await Fixture.CreateAsync();
        var request = Request("Natural", "05", "0923902225");
        request.EstadoCredito = "Normal";
        request.LimiteCredito = 200;
        request.DiasCreditoMaximo = 15;
        var result = await fixture.Service.CreateAsync(request);
        Assert.Equal("AlDia", result.EstadoCredito);
        Assert.False(result.PermiteCredito);
        Assert.Equal(200, result.LimiteCredito);
        Assert.Equal(15, result.DiasCreditoMaximo);
    }

    [Theory]
    [InlineData("facturas@example.invalid", "facturas@example.invalid")]
    [InlineData(null, "contacto@example.invalid")]
    [InlineData("   ", "contacto@example.invalid")]
    public async Task PosUsesBillingEmailWhenSpecifiedOtherwisePrimary(string? storedBilling, string expected)
    {
        await using var fixture = await Fixture.CreateAsync();
        var request = Request("Natural", "05", "0923902225");
        request.CorreoElectronicoPrincipal = "contacto@example.invalid";
        request.CorreoFacturacionElectronica = storedBilling;
        await fixture.Service.CreateAsync(request);
        // También se comprueban blancos heredados, anteriores a la normalización del DTO.
        (await fixture.Db.Clientes.SingleAsync()).CorreoFacturacionElectronica = storedBilling;
        await fixture.Db.SaveChangesAsync();
        var repository = new TestDeIa.Infrastructure.Adapters.Out.Facturacion.EfFacturacionRepository(fixture.Db, fixture.Tenant,
            new TestDeIa.Infrastructure.Adapters.Out.Inventario.EfInventarioRepository(fixture.Db, fixture.Tenant,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<TestDeIa.Infrastructure.Adapters.Out.Inventario.EfInventarioRepository>.Instance),
            new TestDeIa.Infrastructure.Adapters.Out.Saas.SaaSQuotaValidationService(fixture.Db));
        var page = await repository.SearchClientesAsync("0923902225", 0, 10);
        Assert.Equal(expected, Assert.Single(page.Items).Email);
    }

    [Fact]
    public async Task EditingClassificationUpdatesSpecializedRecordWithoutCreatingAnotherPerson()
    {
        await using var fixture = await Fixture.CreateAsync();
        var created = await fixture.Service.CreateAsync(Request("Natural", "05", "0923902225"));
        var updated = await fixture.Service.UpdateAsync(created.Id, Request("Juridico", "04", "1792040531001"));
        Assert.Equal(created.PersonaId, updated!.PersonaId);
        Assert.True((await fixture.Db.Personas.SingleAsync()).EsEmpresa);
        Assert.Empty(await fixture.Db.PersonasNaturales.ToListAsync());
        Assert.Single(await fixture.Db.EmpresasCliente.ToListAsync());
    }

    private sealed class Fixture : IAsyncDisposable
    {
        public TenantContextAccessor Tenant { get; } = new() { EmpresaId = Guid.NewGuid(), UserId = Guid.NewGuid() };
        public TestDeIaDbContext Db { get; }
        public ClienteUseCase Service { get; }
        private Fixture()
        {
            Db = new TestDeIaDbContext(new DbContextOptionsBuilder<TestDeIaDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, Tenant);
            Service = new ClienteUseCase(new EfClienteRepository(Db, Tenant), new EfPersonaRepository(Db, Tenant),
                new EfCatalogoRepository(Db), new User(Tenant.UserId!.Value, Tenant.EmpresaId!.Value));
        }
        public static async Task<Fixture> CreateAsync()
        {
            var fixture = new Fixture();
            fixture.Db.Catalogos.Add(new CatalogoEntity { Id = Guid.NewGuid(), Codigo = "TIPO_IDENTIFICACION", Nombre = "Identificación", IsActive = true,
                Items = new[] { "04", "05", "07" }.Select(x => new CatalogoItemEntity { Id = Guid.NewGuid(), Codigo = x, Nombre = x, IsActive = true }).ToList() });
            await fixture.Db.SaveChangesAsync();
            return fixture;
        }
        public ValueTask DisposeAsync() => Db.DisposeAsync();
    }

    private sealed class User(Guid userId, Guid empresaId) : ICurrentUserAccessor
    {
        public Guid? GetUserId() => userId;
        public Guid GetRequiredUserId() => userId;
        public Guid GetRequiredEmpresaId() => empresaId;
    }
}

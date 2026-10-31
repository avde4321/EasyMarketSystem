using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReengineerPersonaClienteEmpleadoExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DireccionPrincipal",
                table: "Personas",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NombreComercial",
                table: "Personas",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazonSocialONombresCompletos",
                table: "Personas",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Genero",
                table: "Personas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.DropPrimaryKey(
                name: "PK_Empleados",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_PersonaId",
                table: "Empleados");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_PersonaId",
                table: "Clientes");

            migrationBuilder.RenameColumn(
                name: "Telefono",
                table: "Personas",
                newName: "TelefonoCelular");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Personas",
                newName: "CorreoElectronicoPrincipal");

            migrationBuilder.RenameColumn(
                name: "PerfilLaboral",
                table: "Empleados",
                newName: "CargoPuesto");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCreacionId",
                table: "Empleados",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCreacionId",
                table: "Clientes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.Sql(
                """
                UPDATE Personas
                SET TipoIdentificacion = CASE UPPER(COALESCE(TipoIdentificacion, ''))
                    WHEN 'RUC' THEN '04'
                    WHEN 'CEDULA' THEN '05'
                    WHEN 'CÉDULA' THEN '05'
                    WHEN 'PASAPORTE' THEN '06'
                    WHEN 'CONSUMIDOR FINAL' THEN '07'
                    WHEN 'IDENTIFICACION DEL EXTERIOR' THEN '08'
                    WHEN 'IDENTIFICACIÓN DEL EXTERIOR' THEN '08'
                    WHEN 'PLACA' THEN '09'
                    WHEN '04' THEN '04'
                    WHEN '05' THEN '05'
                    WHEN '06' THEN '06'
                    WHEN '07' THEN '07'
                    WHEN '08' THEN '08'
                    WHEN '09' THEN '09'
                    ELSE '05'
                END
                """);

            migrationBuilder.AlterColumn<string>(
                name: "TipoIdentificacion",
                table: "Personas",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
                name: "CodigoBiometrico",
                table: "Empleados",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoEmpleado",
                table: "Empleados",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoLaboral",
                table: "Empleados",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaSalida",
                table: "Empleados",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreContactoEmergencia",
                table: "Empleados",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeComisionVentas",
                table: "Empleados",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SueldoBase",
                table: "Empleados",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TelefonoEmergencia",
                table: "Empleados",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoContrato",
                table: "Empleados",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacionId",
                table: "Empleados",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorreoFacturacionElectronica",
                table: "Clientes",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiasCreditoMaximo",
                table: "Clientes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EsContribuyenteEspecial",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EstadoCredito",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "LimiteCredito",
                table: "Clientes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "ObligadoContabilidad",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteCredito",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TipoCliente",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacionId",
                table: "Clientes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE Personas
                SET RazonSocialONombresCompletos = LTRIM(RTRIM(
                        COALESCE(NULLIF(Nombres, ''), '') +
                        CASE
                            WHEN Nombres IS NOT NULL AND Nombres <> '' AND Apellidos IS NOT NULL AND Apellidos <> '' THEN ' '
                            ELSE ''
                        END +
                        COALESCE(NULLIF(Apellidos, ''), '')
                    )),
                    DireccionPrincipal = COALESCE(NULLIF(Direccion, ''), 'Sin direccion registrada'),
                    TipoIdentificacion = CASE UPPER(TipoIdentificacion)
                        WHEN 'RUC' THEN '04'
                        WHEN 'CEDULA' THEN '05'
                        WHEN 'CÉDULA' THEN '05'
                        WHEN 'PASAPORTE' THEN '06'
                        WHEN 'CONSUMIDOR FINAL' THEN '07'
                        WHEN 'IDENTIFICACION DEL EXTERIOR' THEN '08'
                        WHEN 'IDENTIFICACIÓN DEL EXTERIOR' THEN '08'
                        WHEN 'PLACA' THEN '09'
                        WHEN '04' THEN '04'
                        WHEN '05' THEN '05'
                        WHEN '06' THEN '06'
                        WHEN '07' THEN '07'
                        WHEN '08' THEN '08'
                        WHEN '09' THEN '09'
                        ELSE '05'
                    END
                """);

            migrationBuilder.Sql(
                """
                UPDATE Clientes
                SET UsuarioCreacionId = '33333333-3333-3333-3333-333333333333',
                    TipoCliente = 'Natural',
                    EstadoCredito = 'Normal'
                WHERE UsuarioCreacionId = '00000000-0000-0000-0000-000000000000'
                """);

            migrationBuilder.Sql(
                """
                UPDATE Empleados
                SET UsuarioCreacionId = '33333333-3333-3333-3333-333333333333',
                    TipoContrato = 'Indefinido',
                    EstadoLaboral = 'Activo'
                WHERE UsuarioCreacionId = '00000000-0000-0000-0000-000000000000'
                """);

            migrationBuilder.DropColumn(
                name: "Apellidos",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "Nombres",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "EstadoCivil",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "Cargo",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Clientes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Empleados",
                table: "Empleados",
                column: "PersonaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes",
                column: "PersonaId");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000001"),
                column: "Codigo",
                value: "04");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000002"),
                column: "Codigo",
                value: "05");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000003"),
                column: "Codigo",
                value: "06");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000004"),
                column: "Codigo",
                value: "07");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000005"),
                column: "Codigo",
                value: "08");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000006"),
                column: "Codigo",
                value: "09");

            migrationBuilder.UpdateData(
                table: "Personas",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "DireccionPrincipal", "Genero", "NombreComercial", "RazonSocialONombresCompletos", "TipoIdentificacion" },
                values: new object[] { "Sistema", null, null, "Administrador Sistema", "05" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Empleados",
                table: "Empleados");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DireccionPrincipal",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "NombreComercial",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "RazonSocialONombresCompletos",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "CodigoBiometrico",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "CodigoEmpleado",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "EstadoLaboral",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "FechaSalida",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "NombreContactoEmergencia",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "PorcentajeComisionVentas",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "SueldoBase",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "TelefonoEmergencia",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "TipoContrato",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "UsuarioCreacionId",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacionId",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "CorreoFacturacionElectronica",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DiasCreditoMaximo",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EsContribuyenteEspecial",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EstadoCredito",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "LimiteCredito",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "ObligadoContabilidad",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "PermiteCredito",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TipoCliente",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "UsuarioCreacionId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacionId",
                table: "Clientes");

            migrationBuilder.AddColumn<string>(
                name: "Apellidos",
                table: "Personas",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Personas",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoCivil",
                table: "Personas",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nombres",
                table: "Personas",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cargo",
                table: "Empleados",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Empleados",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Clientes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.RenameColumn(
                name: "TelefonoCelular",
                table: "Personas",
                newName: "Telefono");

            migrationBuilder.RenameColumn(
                name: "CorreoElectronicoPrincipal",
                table: "Personas",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "CargoPuesto",
                table: "Empleados",
                newName: "PerfilLaboral");

            migrationBuilder.AlterColumn<string>(
                name: "TipoIdentificacion",
                table: "Personas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2);

            migrationBuilder.Sql(
                """
                UPDATE Personas
                SET Nombres = RazonSocialONombresCompletos,
                    Apellidos = '',
                    Direccion = DireccionPrincipal
                """);

            migrationBuilder.Sql(
                """
                UPDATE Clientes
                SET Id = NEWID()
                WHERE Id = '00000000-0000-0000-0000-000000000000'
                """);

            migrationBuilder.Sql(
                """
                UPDATE Empleados
                SET Id = NEWID()
                WHERE Id = '00000000-0000-0000-0000-000000000000'
                """);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Empleados",
                table: "Empleados",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000001"),
                column: "Codigo",
                value: "RUC");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000002"),
                column: "Codigo",
                value: "Cedula");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000003"),
                column: "Codigo",
                value: "Pasaporte");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000004"),
                column: "Codigo",
                value: "Consumidor Final");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000005"),
                column: "Codigo",
                value: "Identificacion del Exterior");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000006"),
                column: "Codigo",
                value: "Placa");

            migrationBuilder.UpdateData(
                table: "Personas",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "Apellidos", "Direccion", "Nombres", "TipoIdentificacion" },
                values: new object[] { "Sistema", null, "Administrador", "Sistema" });

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_PersonaId",
                table: "Empleados",
                column: "PersonaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_PersonaId",
                table: "Clientes",
                column: "PersonaId",
                unique: true);
        }
    }
}

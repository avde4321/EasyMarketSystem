using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeSriCatalogInternalCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE CatalogoItems
                SET Codigo = CASE UPPER(LTRIM(RTRIM(Codigo)))
                    WHEN 'PRUEBAS' THEN '1'
                    WHEN 'PRODUCCION' THEN '2'
                    WHEN 'NORMAL' THEN '1'
                    WHEN 'EFECTIVO' THEN '01'
                    WHEN 'COMPENSACION' THEN '15'
                    WHEN 'TARJETA DE DEBITO' THEN '16'
                    WHEN 'DINERO ELECTRONICO' THEN '17'
                    WHEN 'TARJETA PREPAGO' THEN '18'
                    WHEN 'TARJETA DE CREDITO' THEN '19'
                    WHEN 'TRANSFERENCIA' THEN '20'
                    WHEN 'ENDOSO DE TITULOS' THEN '21'
                    ELSE Codigo
                END
                WHERE CatalogoId IN (
                    '70000000-0000-0000-0000-000000000005',
                    '70000000-0000-0000-0000-000000000006',
                    '70000000-0000-0000-0000-000000000007');
                """);

            migrationBuilder.Sql(
                """
                UPDATE EmpresasEmisoras
                SET AmbienteSri = CASE UPPER(LTRIM(RTRIM(AmbienteSri)))
                        WHEN 'PRUEBAS' THEN '1'
                        WHEN 'PRODUCCION' THEN '2'
                        ELSE AmbienteSri
                    END,
                    TipoEmision = CASE UPPER(LTRIM(RTRIM(TipoEmision)))
                        WHEN 'NORMAL' THEN '1'
                        ELSE TipoEmision
                    END;
                """);

            migrationBuilder.Sql(
                """
                UPDATE Facturas
                SET AmbienteSri = CASE UPPER(LTRIM(RTRIM(AmbienteSri)))
                        WHEN 'PRUEBAS' THEN '1'
                        WHEN 'PRODUCCION' THEN '2'
                        ELSE AmbienteSri
                    END,
                    TipoEmision = CASE UPPER(LTRIM(RTRIM(TipoEmision)))
                        WHEN 'NORMAL' THEN '1'
                        ELSE TipoEmision
                    END;
                """);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000041"),
                column: "Codigo",
                value: "1");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000042"),
                column: "Codigo",
                value: "2");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000051"),
                column: "Codigo",
                value: "1");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000061"),
                column: "Codigo",
                value: "01");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000062"),
                column: "Codigo",
                value: "15");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000063"),
                column: "Codigo",
                value: "16");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000064"),
                column: "Codigo",
                value: "17");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000065"),
                column: "Codigo",
                value: "18");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000066"),
                column: "Codigo",
                value: "19");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000067"),
                column: "Codigo",
                value: "20");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000068"),
                column: "Codigo",
                value: "21");

            migrationBuilder.UpdateData(
                table: "EmpresasEmisoras",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "AmbienteSri", "TipoEmision" },
                values: new object[] { "1", "1" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE CatalogoItems
                SET Codigo = CASE LTRIM(RTRIM(Codigo))
                    WHEN '1' THEN CASE
                        WHEN CatalogoId = '70000000-0000-0000-0000-000000000005' THEN 'Pruebas'
                        WHEN CatalogoId = '70000000-0000-0000-0000-000000000006' THEN 'Normal'
                        ELSE Codigo
                    END
                    WHEN '2' THEN 'Produccion'
                    WHEN '01' THEN 'Efectivo'
                    WHEN '15' THEN 'Compensacion'
                    WHEN '16' THEN 'Tarjeta de debito'
                    WHEN '17' THEN 'Dinero electronico'
                    WHEN '18' THEN 'Tarjeta prepago'
                    WHEN '19' THEN 'Tarjeta de credito'
                    WHEN '20' THEN 'Transferencia'
                    WHEN '21' THEN 'Endoso de titulos'
                    ELSE Codigo
                END
                WHERE CatalogoId IN (
                    '70000000-0000-0000-0000-000000000005',
                    '70000000-0000-0000-0000-000000000006',
                    '70000000-0000-0000-0000-000000000007');
                """);

            migrationBuilder.Sql(
                """
                UPDATE EmpresasEmisoras
                SET AmbienteSri = CASE LTRIM(RTRIM(AmbienteSri))
                        WHEN '1' THEN 'Pruebas'
                        WHEN '2' THEN 'Produccion'
                        ELSE AmbienteSri
                    END,
                    TipoEmision = CASE LTRIM(RTRIM(TipoEmision))
                        WHEN '1' THEN 'Normal'
                        ELSE TipoEmision
                    END;
                """);

            migrationBuilder.Sql(
                """
                UPDATE Facturas
                SET AmbienteSri = CASE LTRIM(RTRIM(AmbienteSri))
                        WHEN '1' THEN 'Pruebas'
                        WHEN '2' THEN 'Produccion'
                        ELSE AmbienteSri
                    END,
                    TipoEmision = CASE LTRIM(RTRIM(TipoEmision))
                        WHEN '1' THEN 'Normal'
                        ELSE TipoEmision
                    END;
                """);

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000041"),
                column: "Codigo",
                value: "Pruebas");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000042"),
                column: "Codigo",
                value: "Produccion");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000051"),
                column: "Codigo",
                value: "Normal");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000061"),
                column: "Codigo",
                value: "Efectivo");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000062"),
                column: "Codigo",
                value: "Compensacion");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000063"),
                column: "Codigo",
                value: "Tarjeta de debito");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000064"),
                column: "Codigo",
                value: "Dinero electronico");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000065"),
                column: "Codigo",
                value: "Tarjeta prepago");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000066"),
                column: "Codigo",
                value: "Tarjeta de credito");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000067"),
                column: "Codigo",
                value: "Transferencia");

            migrationBuilder.UpdateData(
                table: "CatalogoItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000068"),
                column: "Codigo",
                value: "Endoso de titulos");

            migrationBuilder.UpdateData(
                table: "EmpresasEmisoras",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "AmbienteSri", "TipoEmision" },
                values: new object[] { "Pruebas", "Normal" });
        }
    }
}

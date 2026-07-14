using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityUserPuntosEmisionAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SecurityUserPuntosEmision",
                columns: table => new
                {
                    SecurityUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaPuntoEmisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityUserPuntosEmision", x => new { x.SecurityUserId, x.EmpresaPuntoEmisionId });
                    table.ForeignKey(
                        name: "FK_SecurityUserPuntosEmision_EmpresaPuntosEmision_EmpresaPuntoEmisionId",
                        column: x => x.EmpresaPuntoEmisionId,
                        principalTable: "EmpresaPuntosEmision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SecurityUserPuntosEmision_SecurityUsers_SecurityUserId",
                        column: x => x.SecurityUserId,
                        principalTable: "SecurityUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityUserPuntosEmision_EmpresaId_EmpresaPuntoEmisionId",
                table: "SecurityUserPuntosEmision",
                columns: new[] { "EmpresaId", "EmpresaPuntoEmisionId" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityUserPuntosEmision_EmpresaId_SecurityUserId",
                table: "SecurityUserPuntosEmision",
                columns: new[] { "EmpresaId", "SecurityUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityUserPuntosEmision_EmpresaPuntoEmisionId",
                table: "SecurityUserPuntosEmision",
                column: "EmpresaPuntoEmisionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecurityUserPuntosEmision");
        }
    }
}

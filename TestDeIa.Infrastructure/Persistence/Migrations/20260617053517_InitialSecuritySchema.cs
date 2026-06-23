using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDeIa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSecuritySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SecurityRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_SecurityUserRoles_SecurityRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "SecurityRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SecurityUserRoles_SecurityUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SecurityUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SecurityRoles",
                columns: new[] { "Id", "IsActive", "Name", "NormalizedName" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), true, "Administrador", "ADMINISTRADOR" });

            migrationBuilder.InsertData(
                table: "SecurityUsers",
                columns: new[] { "Id", "CreatedAt", "DisplayName", "Email", "IsActive", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "UserName" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new DateTimeOffset(new DateTime(2026, 6, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Administrador", "admin@testdeia.local", true, "ADMIN@TESTDEIA.LOCAL", "ADMIN", "0A5BC3E342432F1BAD92FFD51B785343EC72906CDBA6A26131060B008E786656", "admin" });

            migrationBuilder.InsertData(
                table: "SecurityUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityRoles_NormalizedName",
                table: "SecurityRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityUserRoles_RoleId",
                table: "SecurityUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityUsers_NormalizedEmail",
                table: "SecurityUsers",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityUsers_NormalizedUserName",
                table: "SecurityUsers",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecurityUserRoles");

            migrationBuilder.DropTable(
                name: "SecurityRoles");

            migrationBuilder.DropTable(
                name: "SecurityUsers");
        }
    }
}

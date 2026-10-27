using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionTubos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuditoriaYEdicionRemitos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Remitos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AuditoriaRemitos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoId = table.Column<long>(type: "bigint", nullable: false),
                    NumeroRemito = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    FechaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AntesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DespuesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CambiosJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaRemitos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaRemitos_FechaUtc",
                table: "AuditoriaRemitos",
                column: "FechaUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaRemitos_PedidoId_Version",
                table: "AuditoriaRemitos",
                columns: new[] { "PedidoId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaRemitos");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Remitos");
        }
    }
}

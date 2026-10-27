using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionTubos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PlanchadasYConsumoInterno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GasBateria",
                table: "Ubicaciones",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UsosBateria",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TuboId = table.Column<int>(type: "int", nullable: false),
                    UbicacionBateriaId = table.Column<int>(type: "int", nullable: false),
                    UbicacionOrigenId = table.Column<int>(type: "int", nullable: false),
                    UbicacionRetiroId = table.Column<int>(type: "int", nullable: true),
                    CodigoTubo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Gas = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CapacidadNominal = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaIngresoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRetiroUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResultadoRetiro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UsuarioIngreso = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UsuarioIngresoId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UsuarioRetiro = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UsuarioRetiroId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    MotivoRetiro = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsosBateria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsosBateria_Tubos_TuboId",
                        column: x => x.TuboId,
                        principalTable: "Tubos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsosBateria_Ubicaciones_UbicacionBateriaId",
                        column: x => x.UbicacionBateriaId,
                        principalTable: "Ubicaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsosBateria_FechaRetiroUtc_UbicacionBateriaId",
                table: "UsosBateria",
                columns: new[] { "FechaRetiroUtc", "UbicacionBateriaId" });

            migrationBuilder.CreateIndex(
                name: "IX_UsosBateria_TuboId",
                table: "UsosBateria",
                column: "TuboId",
                unique: true,
                filter: "[FechaRetiroUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UsosBateria_UbicacionBateriaId",
                table: "UsosBateria",
                column: "UbicacionBateriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsosBateria");

            migrationBuilder.DropColumn(
                name: "GasBateria",
                table: "Ubicaciones");
        }
    }
}

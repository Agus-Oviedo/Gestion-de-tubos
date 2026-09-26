using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionTubos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RegistroTubos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tubos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoInterno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroSerie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdentificacionGrabada = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TipoGasId = table.Column<int>(type: "int", nullable: false),
                    CapacidadTuboId = table.Column<int>(type: "int", nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Fabricante = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FechaFabricacion = table.Column<DateOnly>(type: "date", nullable: true),
                    TipoPropietario = table.Column<int>(type: "int", nullable: false),
                    ClientePropietarioId = table.Column<int>(type: "int", nullable: true),
                    NombrePropietarioExterno = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EstadoFisico = table.Column<int>(type: "int", nullable: false),
                    EstadoContenido = table.Column<int>(type: "int", nullable: false),
                    EstadoDisponibilidad = table.Column<int>(type: "int", nullable: false),
                    UbicacionId = table.Column<int>(type: "int", nullable: false),
                    FechaAltaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tubos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tubos_CapacidadesTubo_CapacidadTuboId",
                        column: x => x.CapacidadTuboId,
                        principalTable: "CapacidadesTubo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tubos_Clientes_ClientePropietarioId",
                        column: x => x.ClientePropietarioId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tubos_TiposGas_TipoGasId",
                        column: x => x.TipoGasId,
                        principalTable: "TiposGas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tubos_Ubicaciones_UbicacionId",
                        column: x => x.UbicacionId,
                        principalTable: "Ubicaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tubos_CapacidadTuboId",
                table: "Tubos",
                column: "CapacidadTuboId");

            migrationBuilder.CreateIndex(
                name: "IX_Tubos_ClientePropietarioId",
                table: "Tubos",
                column: "ClientePropietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Tubos_CodigoInterno",
                table: "Tubos",
                column: "CodigoInterno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tubos_TipoGasId",
                table: "Tubos",
                column: "TipoGasId");

            migrationBuilder.CreateIndex(
                name: "IX_Tubos_UbicacionId",
                table: "Tubos",
                column: "UbicacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tubos");
        }
    }
}

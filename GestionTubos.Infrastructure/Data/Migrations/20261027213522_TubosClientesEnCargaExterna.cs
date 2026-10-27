using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionTubos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class TubosClientesEnCargaExterna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientePropietarioId",
                table: "EnviosCargaDetalle",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Propietario",
                table: "EnviosCargaDetalle",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "Oxígeno Norte");

            migrationBuilder.AddColumn<int>(
                name: "TipoPropietario",
                table: "EnviosCargaDetalle",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "IngresosClientes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ClienteNombre = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DocumentoCliente = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UbicacionId = table.Column<int>(type: "int", nullable: false),
                    PersonaEntrega = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DocumentoEntrega = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngresosClientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngresosClientes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IngresosClientesDetalle",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IngresoClienteId = table.Column<long>(type: "bigint", nullable: false),
                    TuboId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Gas = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Capacidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaRetiroUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemitoEntregaId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngresosClientesDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngresosClientesDetalle_IngresosClientes_IngresoClienteId",
                        column: x => x.IngresoClienteId,
                        principalTable: "IngresosClientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IngresosClientesDetalle_Remitos_RemitoEntregaId",
                        column: x => x.RemitoEntregaId,
                        principalTable: "Remitos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IngresosClientesDetalle_Tubos_TuboId",
                        column: x => x.TuboId,
                        principalTable: "Tubos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngresosClientes_ClienteId",
                table: "IngresosClientes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_IngresosClientes_Numero",
                table: "IngresosClientes",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IngresosClientesDetalle_IngresoClienteId_TuboId",
                table: "IngresosClientesDetalle",
                columns: new[] { "IngresoClienteId", "TuboId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IngresosClientesDetalle_RemitoEntregaId",
                table: "IngresosClientesDetalle",
                column: "RemitoEntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_IngresosClientesDetalle_TuboId",
                table: "IngresosClientesDetalle",
                column: "TuboId",
                unique: true,
                filter: "[FechaRetiroUtc] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngresosClientesDetalle");

            migrationBuilder.DropTable(
                name: "IngresosClientes");

            migrationBuilder.DropColumn(
                name: "ClientePropietarioId",
                table: "EnviosCargaDetalle");

            migrationBuilder.DropColumn(
                name: "Propietario",
                table: "EnviosCargaDetalle");

            migrationBuilder.DropColumn(
                name: "TipoPropietario",
                table: "EnviosCargaDetalle");
        }
    }
}

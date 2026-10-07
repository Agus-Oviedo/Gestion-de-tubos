using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionTubos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DevolucionesParcialesPorRemito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DevolucionesLote",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RemitoEntregaId = table.Column<long>(type: "bigint", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaConfirmacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UsuarioReceptor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PersonaEntrega = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DniEntrega = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FirmaDataUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevolucionesLote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevolucionesLote_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevolucionesLote_Remitos_RemitoEntregaId",
                        column: x => x.RemitoEntregaId,
                        principalTable: "Remitos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DevolucionesLoteDetalle",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DevolucionLoteId = table.Column<long>(type: "bigint", nullable: false),
                    OperacionCilindroId = table.Column<long>(type: "bigint", nullable: false),
                    EstadoFisico = table.Column<int>(type: "int", nullable: false),
                    EstadoContenido = table.Column<int>(type: "int", nullable: false),
                    UbicacionDestinoId = table.Column<int>(type: "int", nullable: false),
                    RequiereInspeccion = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevolucionesLoteDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevolucionesLoteDetalle_DevolucionesLote_DevolucionLoteId",
                        column: x => x.DevolucionLoteId,
                        principalTable: "DevolucionesLote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DevolucionesLoteDetalle_OperacionesCilindro_OperacionCilindroId",
                        column: x => x.OperacionCilindroId,
                        principalTable: "OperacionesCilindro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesLote_ClienteId",
                table: "DevolucionesLote",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesLote_Numero",
                table: "DevolucionesLote",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesLote_RemitoEntregaId",
                table: "DevolucionesLote",
                column: "RemitoEntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesLoteDetalle_DevolucionLoteId",
                table: "DevolucionesLoteDetalle",
                column: "DevolucionLoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesLoteDetalle_OperacionCilindroId",
                table: "DevolucionesLoteDetalle",
                column: "OperacionCilindroId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevolucionesLoteDetalle");

            migrationBuilder.DropTable(
                name: "DevolucionesLote");
        }
    }
}

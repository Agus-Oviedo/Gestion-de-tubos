using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionTubos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class OperacionesTrazabilidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovimientosCilindro",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TuboId = table.Column<int>(type: "int", nullable: false),
                    FechaHoraUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UbicacionOrigenId = table.Column<int>(type: "int", nullable: true),
                    UbicacionDestinoId = table.Column<int>(type: "int", nullable: true),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    EstadoAnterior = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    EstadoPosterior = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    UsuarioResponsable = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosCilindro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosCilindro_Tubos_TuboId",
                        column: x => x.TuboId,
                        principalTable: "Tubos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperacionesCilindro",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    TuboId = table.Column<int>(type: "int", nullable: false),
                    FechaHoraUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DiasSinCargo = table.Column<int>(type: "int", nullable: true),
                    FechaLimite = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaDevolucionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PersonaRecibe = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperacionesCilindro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperacionesCilindro_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperacionesCilindro_Tubos_TuboId",
                        column: x => x.TuboId,
                        principalTable: "Tubos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PruebasHidraulicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TuboId = table.Column<int>(type: "int", nullable: false),
                    FechaRealizacion = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    Aprobada = table.Column<bool>(type: "bit", nullable: false),
                    EntidadRealizadora = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NumeroCertificado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaRegistroUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PruebasHidraulicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PruebasHidraulicas_Tubos_TuboId",
                        column: x => x.TuboId,
                        principalTable: "Tubos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCilindro_TuboId_FechaHoraUtc",
                table: "MovimientosCilindro",
                columns: new[] { "TuboId", "FechaHoraUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OperacionesCilindro_ClienteId",
                table: "OperacionesCilindro",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_OperacionesCilindro_Numero",
                table: "OperacionesCilindro",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperacionesCilindro_TuboId",
                table: "OperacionesCilindro",
                column: "TuboId");

            migrationBuilder.CreateIndex(
                name: "IX_PruebasHidraulicas_TuboId_FechaVencimiento",
                table: "PruebasHidraulicas",
                columns: new[] { "TuboId", "FechaVencimiento" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosCilindro");

            migrationBuilder.DropTable(
                name: "OperacionesCilindro");

            migrationBuilder.DropTable(
                name: "PruebasHidraulicas");
        }
    }
}

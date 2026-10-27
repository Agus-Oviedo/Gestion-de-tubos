using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionTubos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnviosYRecepcionesCarga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnviosCarga",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Transporte = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaSalidaUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnviosCarga", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnviosCargaDetalle",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnvioCargaId = table.Column<long>(type: "bigint", nullable: false),
                    TuboId = table.Column<int>(type: "int", nullable: false),
                    UbicacionOrigenId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Resolucion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnviosCargaDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnviosCargaDetalle_EnviosCarga_EnvioCargaId",
                        column: x => x.EnvioCargaId,
                        principalTable: "EnviosCarga",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnviosCargaDetalle_Tubos_TuboId",
                        column: x => x.TuboId,
                        principalTable: "Tubos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecepcionesCarga",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EnvioCargaId = table.Column<long>(type: "bigint", nullable: false),
                    FechaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesCarga", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecepcionesCarga_EnviosCarga_EnvioCargaId",
                        column: x => x.EnvioCargaId,
                        principalTable: "EnviosCarga",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecepcionesCargaDetalle",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecepcionCargaId = table.Column<long>(type: "bigint", nullable: false),
                    EnvioCargaDetalleId = table.Column<long>(type: "bigint", nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UbicacionDestinoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesCargaDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecepcionesCargaDetalle_EnviosCargaDetalle_EnvioCargaDetalleId",
                        column: x => x.EnvioCargaDetalleId,
                        principalTable: "EnviosCargaDetalle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesCargaDetalle_RecepcionesCarga_RecepcionCargaId",
                        column: x => x.RecepcionCargaId,
                        principalTable: "RecepcionesCarga",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnviosCarga_Numero",
                table: "EnviosCarga",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnviosCargaDetalle_EnvioCargaId_TuboId",
                table: "EnviosCargaDetalle",
                columns: new[] { "EnvioCargaId", "TuboId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnviosCargaDetalle_TuboId",
                table: "EnviosCargaDetalle",
                column: "TuboId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCarga_EnvioCargaId",
                table: "RecepcionesCarga",
                column: "EnvioCargaId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCarga_Numero",
                table: "RecepcionesCarga",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCargaDetalle_EnvioCargaDetalleId",
                table: "RecepcionesCargaDetalle",
                column: "EnvioCargaDetalleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCargaDetalle_RecepcionCargaId",
                table: "RecepcionesCargaDetalle",
                column: "RecepcionCargaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecepcionesCargaDetalle");

            migrationBuilder.DropTable(
                name: "EnviosCargaDetalle");

            migrationBuilder.DropTable(
                name: "RecepcionesCarga");

            migrationBuilder.DropTable(
                name: "EnviosCarga");
        }
    }
}

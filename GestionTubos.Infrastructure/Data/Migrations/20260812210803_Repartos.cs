using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionTubos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Repartos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Repartos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Responsable = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaSalidaUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreador = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repartos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepartoDetalles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RepartoId = table.Column<long>(type: "bigint", nullable: false),
                    PedidoId = table.Column<long>(type: "bigint", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepartoDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepartoDetalles_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepartoDetalles_Repartos_RepartoId",
                        column: x => x.RepartoId,
                        principalTable: "Repartos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepartoDetalles_PedidoId",
                table: "RepartoDetalles",
                column: "PedidoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepartoDetalles_RepartoId",
                table: "RepartoDetalles",
                column: "RepartoId");

            migrationBuilder.CreateIndex(
                name: "IX_Repartos_Numero",
                table: "Repartos",
                column: "Numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepartoDetalles");

            migrationBuilder.DropTable(
                name: "Repartos");
        }
    }
}

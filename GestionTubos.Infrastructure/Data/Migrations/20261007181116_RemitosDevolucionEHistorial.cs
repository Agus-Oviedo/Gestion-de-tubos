using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionTubos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemitosDevolucionEHistorial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RemitosDevolucion",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OperacionCilindroId = table.Column<long>(type: "bigint", nullable: false),
                    FechaEmisionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UsuarioReceptor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PersonaEntrega = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DniEntrega = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemitosDevolucion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemitosDevolucion_OperacionesCilindro_OperacionCilindroId",
                        column: x => x.OperacionCilindroId,
                        principalTable: "OperacionesCilindro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RemitosDevolucion_Numero",
                table: "RemitosDevolucion",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RemitosDevolucion_OperacionCilindroId",
                table: "RemitosDevolucion",
                column: "OperacionCilindroId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RemitosDevolucion");
        }
    }
}

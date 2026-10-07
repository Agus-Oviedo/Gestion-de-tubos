using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionTubos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class VincularOperacionesConRemitoEntrega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RemitoEntregaId",
                table: "OperacionesCilindro",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE o
                SET RemitoEntregaId = coincidencia.RemitoId
                FROM OperacionesCilindro o
                OUTER APPLY (
                    SELECT TOP (1) r.Id AS RemitoId
                    FROM Remitos r
                    INNER JOIN Pedidos p ON p.Id = r.PedidoId
                    INNER JOIN PedidoDetalles pd ON pd.PedidoId = p.Id AND pd.TuboId = o.TuboId
                    WHERE p.ClienteId = o.ClienteId AND p.Estado = 'Entregado'
                    ORDER BY ABS(DATEDIFF(SECOND, COALESCE(r.FechaFirmaUtc, r.FechaEmisionUtc), o.FechaHoraUtc))
                ) coincidencia
                WHERE o.RemitoEntregaId IS NULL AND coincidencia.RemitoId IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_OperacionesCilindro_RemitoEntregaId",
                table: "OperacionesCilindro",
                column: "RemitoEntregaId");

            migrationBuilder.AddForeignKey(
                name: "FK_OperacionesCilindro_Remitos_RemitoEntregaId",
                table: "OperacionesCilindro",
                column: "RemitoEntregaId",
                principalTable: "Remitos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperacionesCilindro_Remitos_RemitoEntregaId",
                table: "OperacionesCilindro");

            migrationBuilder.DropIndex(
                name: "IX_OperacionesCilindro_RemitoEntregaId",
                table: "OperacionesCilindro");

            migrationBuilder.DropColumn(
                name: "RemitoEntregaId",
                table: "OperacionesCilindro");
        }
    }
}

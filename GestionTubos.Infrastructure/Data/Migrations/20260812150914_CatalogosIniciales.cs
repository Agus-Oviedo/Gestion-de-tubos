using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionTubos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CatalogosIniciales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CapacidadesTubo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapacidadesTubo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposGas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposGas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ubicaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    EsInterna = table.Column<bool>(type: "bit", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ubicaciones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapacidadesTubo_Valor_Unidad",
                table: "CapacidadesTubo",
                columns: new[] { "Valor", "Unidad" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposGas_Nombre",
                table: "TiposGas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ubicaciones_Nombre",
                table: "Ubicaciones",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapacidadesTubo");

            migrationBuilder.DropTable(
                name: "TiposGas");

            migrationBuilder.DropTable(
                name: "Ubicaciones");
        }
    }
}

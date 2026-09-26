using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionTubos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModalidadEntregaPedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactoEntrega",
                table: "Pedidos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DireccionEntregaId",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DireccionEntregaTexto",
                table: "Pedidos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaPrevistaEntrega",
                table: "Pedidos",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModalidadEntrega",
                table: "Pedidos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "RetiroPlanta");

            migrationBuilder.AddColumn<string>(
                name: "TelefonoEntrega",
                table: "Pedidos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactoEntrega",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DireccionEntregaId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DireccionEntregaTexto",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "FechaPrevistaEntrega",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ModalidadEntrega",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "TelefonoEntrega",
                table: "Pedidos");
        }
    }
}

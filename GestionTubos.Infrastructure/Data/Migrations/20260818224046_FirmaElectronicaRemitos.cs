using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionTubos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FirmaElectronicaRemitos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DniRecibe",
                table: "Remitos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFirmaUtc",
                table: "Remitos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirmaDataUrl",
                table: "Remitos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HuellaDocumento",
                table: "Remitos",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonaRecibe",
                table: "Remitos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioEntrega",
                table: "Remitos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DniRecibe",
                table: "Remitos");

            migrationBuilder.DropColumn(
                name: "FechaFirmaUtc",
                table: "Remitos");

            migrationBuilder.DropColumn(
                name: "FirmaDataUrl",
                table: "Remitos");

            migrationBuilder.DropColumn(
                name: "HuellaDocumento",
                table: "Remitos");

            migrationBuilder.DropColumn(
                name: "PersonaRecibe",
                table: "Remitos");

            migrationBuilder.DropColumn(
                name: "UsuarioEntrega",
                table: "Remitos");
        }
    }
}

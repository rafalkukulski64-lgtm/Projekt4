using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projekt4.Migrations
{
    /// <inheritdoc />
    public partial class RenameDiacriticsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Wyposazenie",
                table: "Rooms",
                newName: "Wyposażenie");

            migrationBuilder.RenameColumn(
                name: "Pojemnosc",
                table: "Rooms",
                newName: "Pojemność");

            migrationBuilder.RenameColumn(
                name: "Start",
                table: "Reservations",
                newName: "Początek");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Wyposażenie",
                table: "Rooms",
                newName: "Wyposazenie");

            migrationBuilder.RenameColumn(
                name: "Pojemność",
                table: "Rooms",
                newName: "Pojemnosc");

            migrationBuilder.RenameColumn(
                name: "Początek",
                table: "Reservations",
                newName: "Start");
        }
    }
}

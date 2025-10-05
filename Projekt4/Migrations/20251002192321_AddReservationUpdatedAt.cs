using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projekt4.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataAktualizacji",
                table: "Reservations",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataAktualizacji",
                table: "Reservations");
        }
    }
}

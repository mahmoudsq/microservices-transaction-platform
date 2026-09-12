using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class paymentConfirmedAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorizedAt",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "SettledAt",
                table: "Payments",
                newName: "ConfirmedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConfirmedAt",
                table: "Payments",
                newName: "SettledAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "AuthorizedAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);
        }
    }
}

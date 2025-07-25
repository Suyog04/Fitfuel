using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitfuel.Migrations
{
    /// <inheritdoc />
    public partial class AddLastLogoutAtToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastLogoutAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLogoutAt",
                table: "Users");
        }
    }
}

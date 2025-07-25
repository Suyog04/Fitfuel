using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitfuel.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictedCaloriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PredictedCalories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    PredictedCalories = table.Column<double>(type: "double precision", nullable: false),
                    Duration = table.Column<double>(type: "double precision", nullable: false),
                    HeartRate = table.Column<int>(type: "integer", nullable: false),
                    BodyTemp = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredictedCalories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PredictedCalories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PredictedCalories_UserId",
                table: "PredictedCalories",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PredictedCalories");
        }
    }
}

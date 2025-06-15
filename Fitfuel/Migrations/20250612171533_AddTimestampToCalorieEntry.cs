using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitfuel.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestampToCalorieEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Calories",
                table: "CalorieEntries",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<double>(
                name: "Carbs",
                table: "CalorieEntries",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Fats",
                table: "CalorieEntries",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Fiber",
                table: "CalorieEntries",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Meal",
                table: "CalorieEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Protein",
                table: "CalorieEntries",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WeightInGrams",
                table: "CalorieEntries",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Carbs",
                table: "CalorieEntries");

            migrationBuilder.DropColumn(
                name: "Fats",
                table: "CalorieEntries");

            migrationBuilder.DropColumn(
                name: "Fiber",
                table: "CalorieEntries");

            migrationBuilder.DropColumn(
                name: "Meal",
                table: "CalorieEntries");

            migrationBuilder.DropColumn(
                name: "Protein",
                table: "CalorieEntries");

            migrationBuilder.DropColumn(
                name: "WeightInGrams",
                table: "CalorieEntries");

            migrationBuilder.AlterColumn<int>(
                name: "Calories",
                table: "CalorieEntries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}

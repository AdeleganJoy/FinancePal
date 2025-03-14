using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancePal.Migrations
{
    /// <inheritdoc />
    public partial class FinancePal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "Expenses");

            migrationBuilder.AddColumn<string>(
                name: "Salt",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Expenses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Expenses");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Expenses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}

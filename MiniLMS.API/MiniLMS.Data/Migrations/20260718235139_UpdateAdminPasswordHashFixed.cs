using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenAlpha.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminPasswordHashFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEItvXQGfRq6t4vQm9Jq7kLp3y6+8sFmPZ4y1gNuK9pWq5vB1x2y3z4w5v6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEItvXQGfRq6t4vQm9Jq7kLp3y6+8sFm...");
        }
    }
}

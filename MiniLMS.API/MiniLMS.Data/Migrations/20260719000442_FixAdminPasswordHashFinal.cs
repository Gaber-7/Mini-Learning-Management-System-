using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenAlpha.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixAdminPasswordHashFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEJrNl0D7i1oW5cM1Xz8hQvV9B2kLp3y6+8sFmPZ4y1gNuK9pWq5vB1x2y3z4w5v6==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEFT9UqWvL3qU5Ym1v5m/ZqX9P9v4xW5JbZkLqM1vP4tYn9wPz4Q==");
        }
    }
}

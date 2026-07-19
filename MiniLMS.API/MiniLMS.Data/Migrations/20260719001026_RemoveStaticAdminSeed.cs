using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniLMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStaticAdminSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, "AQAAAAIAAYagAAAAEJrNl0D7i1oW5cM1Xz8hQvV9B2kLp3y6+8sFmPZ4y1gNuK9pWq5vB1x2y3z4w5v6==", "Admin", "admin" });
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenAlpha.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGenAlphaCoursePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "CoursePayments");

            migrationBuilder.DropColumn(
                name: "InstructorShare",
                table: "CoursePayments");

            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "CoursePayments");

            migrationBuilder.DropColumn(
                name: "FinalScorePercentage",
                table: "Certificates");

            migrationBuilder.RenameColumn(
                name: "PlatformShare",
                table: "CoursePayments",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "UsedCount",
                table: "Coupons",
                newName: "MaxUsageCount");

            migrationBuilder.RenameColumn(
                name: "MaxUses",
                table: "Coupons",
                newName: "CurrentUsageCount");

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionPercentage",
                table: "InstructorWallets",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "InstructorWallets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "CoursePayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Coupons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Coupons",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "QrVerificationUrl",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CertificateCode",
                table: "Certificates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "PdfFilePath",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_CourseId",
                table: "Coupons",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_Courses_CourseId",
                table: "Coupons",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coupons_Courses_CourseId",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_CourseId",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "CommissionPercentage",
                table: "InstructorWallets");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "InstructorWallets");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "PdfFilePath",
                table: "Certificates");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "CoursePayments",
                newName: "PlatformShare");

            migrationBuilder.RenameColumn(
                name: "MaxUsageCount",
                table: "Coupons",
                newName: "UsedCount");

            migrationBuilder.RenameColumn(
                name: "CurrentUsageCount",
                table: "Coupons",
                newName: "MaxUses");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "CoursePayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "CoursePayments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InstructorShare",
                table: "CoursePayments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "CoursePayments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "QrVerificationUrl",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CertificateCode",
                table: "Certificates",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalScorePercentage",
                table: "Certificates",
                type: "decimal(5,2)",
                nullable: true);
        }
    }
}

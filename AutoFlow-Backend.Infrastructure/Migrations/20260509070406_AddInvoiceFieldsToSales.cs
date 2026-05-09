using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFlow_Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceFieldsToSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvoiceEmail",
                table: "Sales",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "Sales",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceSentAt",
                table: "Sales",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceEmail",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "InvoiceSentAt",
                table: "Sales");
        }
    }
}

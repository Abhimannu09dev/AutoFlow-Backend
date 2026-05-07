using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFlow_Background.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMileageToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Mileage",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mileage",
                table: "Vehicles");
        }
    }
}

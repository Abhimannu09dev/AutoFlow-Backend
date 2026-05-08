using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFlow_Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally no-op.
            // Identity and Vehicles tables are created in 20260430172608_SyncModelChanges.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally no-op.
        }
    }
}

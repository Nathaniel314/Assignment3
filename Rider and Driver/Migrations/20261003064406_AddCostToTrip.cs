using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rider_and_Driver.Migrations
{
    /// <inheritdoc />
    public partial class AddCostToTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "Trips",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Trips");
        }
    }
}

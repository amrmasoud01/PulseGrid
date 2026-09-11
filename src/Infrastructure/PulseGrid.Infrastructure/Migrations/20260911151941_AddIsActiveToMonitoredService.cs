using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulseGrid.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToMonitoredService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "MonitoredServices",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "MonitoredServices");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulseGrid.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitoredServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TargetUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    IntervalSeconds = table.Column<int>(type: "int", nullable: false),
                    TimeoutMs = table.Column<int>(type: "int", nullable: false),
                    CurrentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastCheckedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoredServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthCheckLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: true),
                    LatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    CheckedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthCheckLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthCheckLogs_MonitoredServices_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "MonitoredServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_MonitoredServices_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "MonitoredServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentUpdates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    StatusSnapshot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuthorUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentUpdates_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthCheckLogs_CheckedAtUtc",
                table: "HealthCheckLogs",
                column: "CheckedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCheckLogs_ServiceId_CheckedAtUtc",
                table: "HealthCheckLogs",
                columns: new[] { "ServiceId", "CheckedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_CreatedAtUtc",
                table: "Incidents",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_ServiceId",
                table: "Incidents",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentUpdates_IncidentId",
                table: "IncidentUpdates",
                column: "IncidentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthCheckLogs");

            migrationBuilder.DropTable(
                name: "IncidentUpdates");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "MonitoredServices");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auditarium.Dal.SqlServer.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddJobRuntimeState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "job_runtime_state",
                schema: "auditarium",
                columns: table => new
                {
                    job_key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    last_started_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    last_completed_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    last_result = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    running_instance_id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    lease_until = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    last_startup_version = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    concurrency_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_runtime_state", x => x.job_key);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_runtime_state",
                schema: "auditarium");
        }
    }
}

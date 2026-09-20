using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auditarium.Dal.SqlServer.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "system_audit_log",
                schema: "auditarium",
                columns: table => new
                {
                    event_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    occurred_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    action = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    object_type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    object_id = table.Column<long>(type: "bigint", nullable: true),
                    before_state = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    after_state = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_audit_log", x => x.event_id);
                    table.ForeignKey(
                        name: "FK_system_audit_log_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "auditarium",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_system_audit_log_object_type_object_id_action_occurred_at",
                schema: "auditarium",
                table: "system_audit_log",
                columns: new[] { "object_type", "object_id", "action", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_system_audit_log_user_id",
                schema: "auditarium",
                table: "system_audit_log",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_audit_log",
                schema: "auditarium");
        }
    }
}

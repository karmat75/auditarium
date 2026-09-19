using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auditarium.Dal.SqlServer.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddApiCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "api_credentials",
                schema: "auditarium",
                columns: table => new
                {
                    api_credential_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    identity_id = table.Column<long>(type: "bigint", nullable: false),
                    key_id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    secret_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    last_used_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_credentials", x => x.api_credential_id);
                    table.ForeignKey(
                        name: "FK_api_credentials_user_identities_identity_id",
                        column: x => x.identity_id,
                        principalSchema: "auditarium",
                        principalTable: "user_identities",
                        principalColumn: "identity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_api_credentials_identity_id",
                schema: "auditarium",
                table: "api_credentials",
                column: "identity_id");

            migrationBuilder.CreateIndex(
                name: "IX_api_credentials_key_id",
                schema: "auditarium",
                table: "api_credentials",
                column: "key_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_credentials",
                schema: "auditarium");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Auditarium.Dal.PostgreSql.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialBootstrap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auditarium");

            migrationBuilder.CreateTable(
                name: "application_settings",
                schema: "auditarium",
                columns: table => new
                {
                    setting_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    serialized_value = table.Column<string>(type: "text", nullable: false),
                    concurrency_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_settings", x => x.setting_key);
                });

            migrationBuilder.CreateTable(
                name: "authentication_providers",
                schema: "auditarium",
                columns: table => new
                {
                    authentication_provider_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    provider_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    provider_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    concurrency_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authentication_providers", x => x.authentication_provider_id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "auditarium",
                columns: table => new
                {
                    permission_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.permission_key);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "auditarium",
                columns: table => new
                {
                    role_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    concurrency_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "auditarium",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    concurrency_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "auditarium",
                columns: table => new
                {
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    permission_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => new { x.role_id, x.permission_key });
                    table.ForeignKey(
                        name: "FK_role_permissions_permissions_permission_key",
                        column: x => x.permission_key,
                        principalSchema: "auditarium",
                        principalTable: "permissions",
                        principalColumn: "permission_key",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "auditarium",
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_identities",
                schema: "auditarium",
                columns: table => new
                {
                    identity_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    authentication_provider_id = table.Column<long>(type: "bigint", nullable: false),
                    external_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_identities", x => x.identity_id);
                    table.ForeignKey(
                        name: "FK_user_identities_authentication_providers_authentication_pro~",
                        column: x => x.authentication_provider_id,
                        principalSchema: "auditarium",
                        principalTable: "authentication_providers",
                        principalColumn: "authentication_provider_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_identities_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "auditarium",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "auditarium",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "auditarium",
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "auditarium",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "local_credentials",
                schema: "auditarium",
                columns: table => new
                {
                    identity_id = table.Column<long>(type: "bigint", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    password_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false),
                    failed_attempt_count = table.Column<int>(type: "integer", nullable: false),
                    failed_attempt_window_started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_local_credentials", x => x.identity_id);
                    table.ForeignKey(
                        name: "FK_local_credentials_user_identities_identity_id",
                        column: x => x.identity_id,
                        principalSchema: "auditarium",
                        principalTable: "user_identities",
                        principalColumn: "identity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_authentication_providers_provider_key",
                schema: "auditarium",
                table: "authentication_providers",
                column: "provider_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_permission_key",
                schema: "auditarium",
                table: "role_permissions",
                column: "permission_key");

            migrationBuilder.CreateIndex(
                name: "IX_roles_role_key",
                schema: "auditarium",
                table: "roles",
                column: "role_key",
                unique: true,
                filter: "role_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_user_identities_authentication_provider_id_external_id",
                schema: "auditarium",
                table: "user_identities",
                columns: new[] { "authentication_provider_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_identities_user_id",
                schema: "auditarium",
                table: "user_identities",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                schema: "auditarium",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_user_key",
                schema: "auditarium",
                table: "users",
                column: "user_key",
                unique: true,
                filter: "user_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                schema: "auditarium",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_settings",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "local_credentials",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "user_identities",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "authentication_providers",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "users",
                schema: "auditarium");
        }
    }
}

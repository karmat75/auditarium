using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auditarium.Dal.PostgreSql.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documents",
                schema: "auditarium",
                columns: table => new
                {
                    document_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    publisher = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    version = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    publication_date = table.Column<DateOnly>(type: "date", nullable: true),
                    source = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    usage_state = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    usage_state_reason = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    concurrency_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.document_id);
                    table.CheckConstraint("ck_documents_deprecated_reason", "usage_state <> 'Deprecated' OR usage_state_reason IS NOT NULL");
                    table.CheckConstraint("ck_documents_usage_state", "usage_state IN ('Active', 'Deprecated')");
                });

            migrationBuilder.CreateTable(
                name: "file_items",
                schema: "auditarium",
                columns: table => new
                {
                    file_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    original_file_name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    save_file_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    extension = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    save_file_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    content_type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    checksum = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file_items", x => x.file_id);
                    table.ForeignKey(
                        name: "FK_file_items_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "auditarium",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "scope_types",
                schema: "auditarium",
                columns: table => new
                {
                    scope_type_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scope_types", x => x.scope_type_id);
                });

            migrationBuilder.CreateTable(
                name: "catalog_versions",
                schema: "auditarium",
                columns: table => new
                {
                    catalog_version_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    document_id = table.Column<long>(type: "bigint", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    catalog_state = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    draft_revision = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    source_file_id = table.Column<long>(type: "bigint", nullable: true),
                    concurrency_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_versions", x => x.catalog_version_id);
                    table.CheckConstraint("ck_catalog_versions_numbers", "version_number >= 1 AND draft_revision >= 1");
                    table.CheckConstraint("ck_catalog_versions_state", "catalog_state IN ('Draft', 'Ready')");
                    table.ForeignKey(
                        name: "FK_catalog_versions_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "auditarium",
                        principalTable: "documents",
                        principalColumn: "document_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_catalog_versions_file_items_source_file_id",
                        column: x => x.source_file_id,
                        principalSchema: "auditarium",
                        principalTable: "file_items",
                        principalColumn: "file_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_catalog_versions_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "auditarium",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "document_elements",
                schema: "auditarium",
                columns: table => new
                {
                    element_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    catalog_version_id = table.Column<long>(type: "bigint", nullable: false),
                    parent_element_id = table.Column<long>(type: "bigint", nullable: true),
                    title = table.Column<string>(type: "text", nullable: true),
                    text = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_elements", x => x.element_id);
                    table.CheckConstraint("ck_document_elements_values", "sort_order >= 0 AND parent_element_id <> element_id AND (title IS NOT NULL OR text IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_document_elements_catalog_versions_catalog_version_id",
                        column: x => x.catalog_version_id,
                        principalSchema: "auditarium",
                        principalTable: "catalog_versions",
                        principalColumn: "catalog_version_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_elements_document_elements_parent_element_id",
                        column: x => x.parent_element_id,
                        principalSchema: "auditarium",
                        principalTable: "document_elements",
                        principalColumn: "element_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "document_element_weights",
                schema: "auditarium",
                columns: table => new
                {
                    element_id = table.Column<long>(type: "bigint", nullable: false),
                    weight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_element_weights", x => x.element_id);
                    table.CheckConstraint("ck_document_element_weights_value", "weight IN (1, 2, 4, 5)");
                    table.ForeignKey(
                        name: "FK_document_element_weights_document_elements_element_id",
                        column: x => x.element_id,
                        principalSchema: "auditarium",
                        principalTable: "document_elements",
                        principalColumn: "element_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                schema: "auditarium",
                columns: table => new
                {
                    question_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    element_id = table.Column<long>(type: "bigint", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    verification_hint = table.Column<string>(type: "text", nullable: true),
                    evidence_hint = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.question_id);
                    table.CheckConstraint("ck_questions_values", "sort_order >= 0 AND text <> ''");
                    table.ForeignKey(
                        name: "FK_questions_document_elements_element_id",
                        column: x => x.element_id,
                        principalSchema: "auditarium",
                        principalTable: "document_elements",
                        principalColumn: "element_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "question_scope_types",
                schema: "auditarium",
                columns: table => new
                {
                    question_id = table.Column<long>(type: "bigint", nullable: false),
                    scope_type_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_scope_types", x => new { x.question_id, x.scope_type_id });
                    table.ForeignKey(
                        name: "FK_question_scope_types_questions_question_id",
                        column: x => x.question_id,
                        principalSchema: "auditarium",
                        principalTable: "questions",
                        principalColumn: "question_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_question_scope_types_scope_types_scope_type_id",
                        column: x => x.scope_type_id,
                        principalSchema: "auditarium",
                        principalTable: "scope_types",
                        principalColumn: "scope_type_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "auditarium",
                table: "scope_types",
                columns: new[] { "scope_type_id", "description", "key", "name" },
                values: new object[,]
                {
                    { 1L, "Organisatorisch abgegrenzte Gesamteinheit.", "ORGANIZATION", "Organisation" },
                    { 2L, "Räumlich zusammenhängende Betriebsstätte.", "SITE", "Standort" },
                    { 3L, "Einzelnes baulich abgegrenztes Gebäude.", "BUILDING", "Gebäude" },
                    { 4L, "Organisatorisch oder funktional abgegrenzter Bereich.", "AREA", "Bereich" },
                    { 5L, "Einzelner räumlich abgegrenzter Raum.", "ROOM", "Raum" },
                    { 6L, "Bereich für technische Infrastruktur.", "TECHNICAL_AREA", "Technikbereich" },
                    { 7L, "Abgegrenzte Kommunikationsinfrastruktur.", "NETWORK", "Netzwerk" },
                    { 8L, "Konkretes technisches System oder Plattform.", "IT_SYSTEM", "IT-System" },
                    { 9L, "Softwareanwendung oder Softwaresystem.", "APPLICATION", "Anwendung" },
                    { 10L, "Definierter organisatorischer oder technischer Ablauf.", "PROCESS", "Prozess" },
                    { 11L, "Bereitgestellte technische oder organisatorische Leistung.", "SERVICE", "Dienst / Service" },
                    { 12L, "Externe Organisation oder Vertragspartner.", "EXTERNAL_PROVIDER", "Externer Dienstleister" },
                    { 13L, "Nur verwenden, wenn kein anderer Scope Type passt.", "OTHER", "Sonstiges" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_versions_created_by",
                schema: "auditarium",
                table: "catalog_versions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_versions_document_id_version_number",
                schema: "auditarium",
                table: "catalog_versions",
                columns: new[] { "document_id", "version_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_versions_source_file_id",
                schema: "auditarium",
                table: "catalog_versions",
                column: "source_file_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_elements_catalog_version_id_sort_order",
                schema: "auditarium",
                table: "document_elements",
                columns: new[] { "catalog_version_id", "sort_order" },
                unique: true,
                filter: "parent_element_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_document_elements_parent_element_id_sort_order",
                schema: "auditarium",
                table: "document_elements",
                columns: new[] { "parent_element_id", "sort_order" },
                unique: true,
                filter: "parent_element_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_file_items_created_by",
                schema: "auditarium",
                table: "file_items",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_question_scope_types_scope_type_id",
                schema: "auditarium",
                table: "question_scope_types",
                column: "scope_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_element_id_sort_order",
                schema: "auditarium",
                table: "questions",
                columns: new[] { "element_id", "sort_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scope_types_key",
                schema: "auditarium",
                table: "scope_types",
                column: "key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_element_weights",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "question_scope_types",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "questions",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "scope_types",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "document_elements",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "catalog_versions",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "file_items",
                schema: "auditarium");
        }
    }
}

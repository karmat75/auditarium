using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auditarium.Dal.SqlServer.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_units",
                schema: "auditarium",
                columns: table => new
                {
                    audit_unit_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    parent_audit_unit_id = table.Column<long>(type: "bigint", nullable: true),
                    scope_type_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    usage_state = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    usage_state_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    concurrency_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_units", x => x.audit_unit_id);
                    table.CheckConstraint("ck_audit_units_values", "parent_audit_unit_id <> audit_unit_id AND usage_state IN ('Active', 'Inactive') AND (usage_state <> 'Inactive' OR usage_state_reason IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_audit_units_audit_units_parent_audit_unit_id",
                        column: x => x.parent_audit_unit_id,
                        principalSchema: "auditarium",
                        principalTable: "audit_units",
                        principalColumn: "audit_unit_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_audit_units_scope_types_scope_type_id",
                        column: x => x.scope_type_id,
                        principalSchema: "auditarium",
                        principalTable: "scope_types",
                        principalColumn: "scope_type_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audits",
                schema: "auditarium",
                columns: table => new
                {
                    audit_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    origin_audit_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    audit_unit_id = table.Column<long>(type: "bigint", nullable: false),
                    catalog_version_id = table.Column<long>(type: "bigint", nullable: false),
                    audit_unit_context = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    audit_state = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    state_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    assigned_auditor_user_id = table.Column<long>(type: "bigint", nullable: true),
                    audit_settings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    concurrency_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audits", x => x.audit_id);
                    table.CheckConstraint("ck_audits_values", "origin_audit_id <> audit_id AND audit_state IN ('Draft', 'Ready', 'InProgress', 'Finalized', 'Canceled') AND ((audit_state = 'Draft' AND audit_unit_context IS NULL AND assigned_auditor_user_id IS NULL) OR audit_state <> 'Draft') AND (audit_state = 'Draft' OR audit_unit_context IS NOT NULL) AND (audit_state <> 'Finalized' OR assigned_auditor_user_id IS NULL) AND (audit_state <> 'Canceled' OR state_reason IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_audits_audit_units_audit_unit_id",
                        column: x => x.audit_unit_id,
                        principalSchema: "auditarium",
                        principalTable: "audit_units",
                        principalColumn: "audit_unit_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_audits_audits_origin_audit_id",
                        column: x => x.origin_audit_id,
                        principalSchema: "auditarium",
                        principalTable: "audits",
                        principalColumn: "audit_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_audits_catalog_versions_catalog_version_id",
                        column: x => x.catalog_version_id,
                        principalSchema: "auditarium",
                        principalTable: "catalog_versions",
                        principalColumn: "catalog_version_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_audits_users_assigned_auditor_user_id",
                        column: x => x.assigned_auditor_user_id,
                        principalSchema: "auditarium",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_document_elements",
                schema: "auditarium",
                columns: table => new
                {
                    audit_document_element_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    audit_id = table.Column<long>(type: "bigint", nullable: false),
                    element_id = table.Column<long>(type: "bigint", nullable: false),
                    weight_snapshot = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_document_elements", x => x.audit_document_element_id);
                    table.CheckConstraint("ck_audit_document_elements_weight", "weight_snapshot BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_audit_document_elements_audits_audit_id",
                        column: x => x.audit_id,
                        principalSchema: "auditarium",
                        principalTable: "audits",
                        principalColumn: "audit_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_audit_document_elements_document_elements_element_id",
                        column: x => x.element_id,
                        principalSchema: "auditarium",
                        principalTable: "document_elements",
                        principalColumn: "element_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_questions",
                schema: "auditarium",
                columns: table => new
                {
                    audit_question_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    audit_document_element_id = table.Column<long>(type: "bigint", nullable: false),
                    question_id = table.Column<long>(type: "bigint", nullable: false),
                    result = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    evidence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    answered_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    answered_by = table.Column<long>(type: "bigint", nullable: true),
                    concurrency_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_questions", x => x.audit_question_id);
                    table.CheckConstraint("ck_audit_questions_answer", "(result IS NULL AND comment IS NULL AND evidence IS NULL AND answered_at IS NULL AND answered_by IS NULL) OR (result IS NOT NULL AND answered_at IS NOT NULL AND answered_by IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_audit_questions_audit_document_elements_audit_document_element_id",
                        column: x => x.audit_document_element_id,
                        principalSchema: "auditarium",
                        principalTable: "audit_document_elements",
                        principalColumn: "audit_document_element_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_audit_questions_questions_question_id",
                        column: x => x.question_id,
                        principalSchema: "auditarium",
                        principalTable: "questions",
                        principalColumn: "question_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_audit_questions_users_answered_by",
                        column: x => x.answered_by,
                        principalSchema: "auditarium",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_document_elements_audit_id_element_id",
                schema: "auditarium",
                table: "audit_document_elements",
                columns: new[] { "audit_id", "element_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_document_elements_element_id",
                schema: "auditarium",
                table: "audit_document_elements",
                column: "element_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_questions_answered_by",
                schema: "auditarium",
                table: "audit_questions",
                column: "answered_by");

            migrationBuilder.CreateIndex(
                name: "IX_audit_questions_audit_document_element_id_question_id",
                schema: "auditarium",
                table: "audit_questions",
                columns: new[] { "audit_document_element_id", "question_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_questions_question_id",
                schema: "auditarium",
                table: "audit_questions",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_units_parent_audit_unit_id",
                schema: "auditarium",
                table: "audit_units",
                column: "parent_audit_unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_units_scope_type_id",
                schema: "auditarium",
                table: "audit_units",
                column: "scope_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_audits_assigned_auditor_user_id",
                schema: "auditarium",
                table: "audits",
                column: "assigned_auditor_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_audits_audit_unit_id",
                schema: "auditarium",
                table: "audits",
                column: "audit_unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_audits_catalog_version_id",
                schema: "auditarium",
                table: "audits",
                column: "catalog_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_audits_origin_audit_id",
                schema: "auditarium",
                table: "audits",
                column: "origin_audit_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_questions",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "audit_document_elements",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "audits",
                schema: "auditarium");

            migrationBuilder.DropTable(
                name: "audit_units",
                schema: "auditarium");
        }
    }
}

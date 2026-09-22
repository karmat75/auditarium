using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auditarium.Dal.SqlServer.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteRetention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "auditarium",
                table: "documents",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "deleted_by",
                schema: "auditarium",
                table: "documents",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deletion_reason",
                schema: "auditarium",
                table: "documents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "auditarium",
                table: "audits",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "deleted_by",
                schema: "auditarium",
                table: "audits",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deletion_reason",
                schema: "auditarium",
                table: "audits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "auditarium",
                table: "audit_units",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "deleted_by",
                schema: "auditarium",
                table: "audit_units",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deletion_reason",
                schema: "auditarium",
                table: "audit_units",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_documents_deleted_at",
                schema: "auditarium",
                table: "documents",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_documents_deleted_by",
                schema: "auditarium",
                table: "documents",
                column: "deleted_by");

            migrationBuilder.AddCheckConstraint(
                name: "ck_documents_soft_delete",
                schema: "auditarium",
                table: "documents",
                sql: "(deleted_at IS NULL AND deleted_by IS NULL AND deletion_reason IS NULL) OR (deleted_at IS NOT NULL AND deleted_by IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_audits_deleted_at",
                schema: "auditarium",
                table: "audits",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_audits_deleted_by",
                schema: "auditarium",
                table: "audits",
                column: "deleted_by");

            migrationBuilder.AddCheckConstraint(
                name: "ck_audits_soft_delete",
                schema: "auditarium",
                table: "audits",
                sql: "(deleted_at IS NULL AND deleted_by IS NULL AND deletion_reason IS NULL) OR (deleted_at IS NOT NULL AND deleted_by IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_audit_units_deleted_at",
                schema: "auditarium",
                table: "audit_units",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_audit_units_deleted_by",
                schema: "auditarium",
                table: "audit_units",
                column: "deleted_by");

            migrationBuilder.AddCheckConstraint(
                name: "ck_audit_units_soft_delete",
                schema: "auditarium",
                table: "audit_units",
                sql: "(deleted_at IS NULL AND deleted_by IS NULL AND deletion_reason IS NULL) OR (deleted_at IS NOT NULL AND deleted_by IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_audit_units_users_deleted_by",
                schema: "auditarium",
                table: "audit_units",
                column: "deleted_by",
                principalSchema: "auditarium",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_audits_users_deleted_by",
                schema: "auditarium",
                table: "audits",
                column: "deleted_by",
                principalSchema: "auditarium",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_documents_users_deleted_by",
                schema: "auditarium",
                table: "documents",
                column: "deleted_by",
                principalSchema: "auditarium",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_audit_units_users_deleted_by",
                schema: "auditarium",
                table: "audit_units");

            migrationBuilder.DropForeignKey(
                name: "FK_audits_users_deleted_by",
                schema: "auditarium",
                table: "audits");

            migrationBuilder.DropForeignKey(
                name: "FK_documents_users_deleted_by",
                schema: "auditarium",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "IX_documents_deleted_at",
                schema: "auditarium",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "IX_documents_deleted_by",
                schema: "auditarium",
                table: "documents");

            migrationBuilder.DropCheckConstraint(
                name: "ck_documents_soft_delete",
                schema: "auditarium",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "IX_audits_deleted_at",
                schema: "auditarium",
                table: "audits");

            migrationBuilder.DropIndex(
                name: "IX_audits_deleted_by",
                schema: "auditarium",
                table: "audits");

            migrationBuilder.DropCheckConstraint(
                name: "ck_audits_soft_delete",
                schema: "auditarium",
                table: "audits");

            migrationBuilder.DropIndex(
                name: "IX_audit_units_deleted_at",
                schema: "auditarium",
                table: "audit_units");

            migrationBuilder.DropIndex(
                name: "IX_audit_units_deleted_by",
                schema: "auditarium",
                table: "audit_units");

            migrationBuilder.DropCheckConstraint(
                name: "ck_audit_units_soft_delete",
                schema: "auditarium",
                table: "audit_units");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "auditarium",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "auditarium",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "deletion_reason",
                schema: "auditarium",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "auditarium",
                table: "audits");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "auditarium",
                table: "audits");

            migrationBuilder.DropColumn(
                name: "deletion_reason",
                schema: "auditarium",
                table: "audits");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "auditarium",
                table: "audit_units");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "auditarium",
                table: "audit_units");

            migrationBuilder.DropColumn(
                name: "deletion_reason",
                schema: "auditarium",
                table: "audit_units");
        }
    }
}

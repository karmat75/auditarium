using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auditarium.Dal.SqlServer.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalCredentialConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "concurrency_version",
                schema: "auditarium",
                table: "local_credentials",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "concurrency_version",
                schema: "auditarium",
                table: "local_credentials");
        }
    }
}

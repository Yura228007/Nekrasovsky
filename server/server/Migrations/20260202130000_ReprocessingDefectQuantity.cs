using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class ReprocessingDefectQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: add column only if not present (fixes broken migration history)
            migrationBuilder.Sql(@"
                ALTER TABLE ""Reprocessing"" ADD COLUMN IF NOT EXISTS ""DefectQuantity"" integer NOT NULL DEFAULT 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefectQuantity",
                table: "Reprocessing");
        }
    }
}

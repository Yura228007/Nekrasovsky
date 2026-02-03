using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class ProductOutputRewindQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: add column only if not present (fixes broken migration history)
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductOutput"" ADD COLUMN IF NOT EXISTS ""RewindQuantity"" integer NOT NULL DEFAULT 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RewindQuantity",
                table: "ProductOutput");
        }
    }
}

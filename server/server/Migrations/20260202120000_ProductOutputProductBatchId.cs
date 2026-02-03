using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class ProductOutputProductBatchId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: add column/index/FK only if not present (fixes broken migration history)
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductOutput"" ADD COLUMN IF NOT EXISTS ""ProductBatchId"" integer NULL;
                CREATE INDEX IF NOT EXISTS ""IX_ProductOutput_ProductBatchId"" ON ""ProductOutput"" (""ProductBatchId"");
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_ProductOutput_ProductBatch_ProductBatchId') THEN
                        ALTER TABLE ""ProductOutput"" ADD CONSTRAINT ""FK_ProductOutput_ProductBatch_ProductBatchId""
                            FOREIGN KEY (""ProductBatchId"") REFERENCES ""ProductBatch"" (""Id"") ON DELETE SET NULL;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductOutput_ProductBatch_ProductBatchId",
                table: "ProductOutput");

            migrationBuilder.DropIndex(
                name: "IX_ProductOutput_ProductBatchId",
                table: "ProductOutput");

            migrationBuilder.DropColumn(
                name: "ProductBatchId",
                table: "ProductOutput");
        }
    }
}

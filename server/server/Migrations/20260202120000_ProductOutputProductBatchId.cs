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
            migrationBuilder.AddColumn<int>(
                name: "ProductBatchId",
                table: "ProductOutput",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_ProductBatchId",
                table: "ProductOutput",
                column: "ProductBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductOutput_ProductBatch_ProductBatchId",
                table: "ProductOutput",
                column: "ProductBatchId",
                principalTable: "ProductBatch",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
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

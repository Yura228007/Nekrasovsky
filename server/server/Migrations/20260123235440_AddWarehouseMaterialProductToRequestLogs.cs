using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseMaterialProductToRequestLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaterialId",
                table: "RequestLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "RequestLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "RequestLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_MaterialId",
                table: "RequestLogs",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_ProductId",
                table: "RequestLogs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_WarehouseId",
                table: "RequestLogs",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestLogs_Material_MaterialId",
                table: "RequestLogs",
                column: "MaterialId",
                principalTable: "Material",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestLogs_Product_ProductId",
                table: "RequestLogs",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestLogs_Warehouse_WarehouseId",
                table: "RequestLogs",
                column: "WarehouseId",
                principalTable: "Warehouse",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestLogs_Material_MaterialId",
                table: "RequestLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestLogs_Product_ProductId",
                table: "RequestLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestLogs_Warehouse_WarehouseId",
                table: "RequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestLogs_MaterialId",
                table: "RequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestLogs_ProductId",
                table: "RequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestLogs_WarehouseId",
                table: "RequestLogs");

            migrationBuilder.DropColumn(
                name: "MaterialId",
                table: "RequestLogs");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "RequestLogs");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "RequestLogs");
        }
    }
}

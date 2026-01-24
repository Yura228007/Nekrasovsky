using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    public partial class AddProductIdToFillingWarehouse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FillingWarehouse",
                table: "FillingWarehouse");

            migrationBuilder.DropForeignKey(
                name: "FK_FillingWarehouse_Material_MaterialId",
                table: "FillingWarehouse");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "FillingWarehouse",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"CREATE SEQUENCE IF NOT EXISTS ""FillingWarehouse_Id_seq"";");
            migrationBuilder.Sql(@"UPDATE ""FillingWarehouse"" SET ""Id"" = nextval('""FillingWarehouse_Id_seq""') WHERE ""Id"" IS NULL;");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "FillingWarehouse",
                type: "integer",
                nullable: false);

            migrationBuilder.Sql(@"ALTER TABLE ""FillingWarehouse"" ALTER COLUMN ""Id"" SET DEFAULT nextval('""FillingWarehouse_Id_seq""');");

            migrationBuilder.AlterColumn<int>(
                name: "MaterialId",
                table: "FillingWarehouse",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "FillingWarehouse",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FillingWarehouse",
                table: "FillingWarehouse",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FillingWarehouse_WarehouseId_MaterialId",
                table: "FillingWarehouse",
                columns: new[] { "WarehouseId", "MaterialId" },
                unique: true,
                filter: "\"MaterialId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FillingWarehouse_WarehouseId_ProductId",
                table: "FillingWarehouse",
                columns: new[] { "WarehouseId", "ProductId" },
                unique: true,
                filter: "\"ProductId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_FillingWarehouse_Material_MaterialId",
                table: "FillingWarehouse",
                column: "MaterialId",
                principalTable: "Material",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FillingWarehouse_Product_ProductId",
                table: "FillingWarehouse",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddCheckConstraint(
                name: "CK_FillingWarehouse_MaterialOrProduct",
                table: "FillingWarehouse",
                sql: "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FillingWarehouse_MaterialOrProduct",
                table: "FillingWarehouse");

            migrationBuilder.DropForeignKey(
                name: "FK_FillingWarehouse_Product_ProductId",
                table: "FillingWarehouse");

            migrationBuilder.DropForeignKey(
                name: "FK_FillingWarehouse_Material_MaterialId",
                table: "FillingWarehouse");

            migrationBuilder.DropIndex(
                name: "IX_FillingWarehouse_WarehouseId_ProductId",
                table: "FillingWarehouse");

            migrationBuilder.DropIndex(
                name: "IX_FillingWarehouse_WarehouseId_MaterialId",
                table: "FillingWarehouse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FillingWarehouse",
                table: "FillingWarehouse");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "FillingWarehouse");

            migrationBuilder.AlterColumn<int>(
                name: "MaterialId",
                table: "FillingWarehouse",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FillingWarehouse");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FillingWarehouse",
                table: "FillingWarehouse",
                columns: new[] { "WarehouseId", "MaterialId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FillingWarehouse_Material_MaterialId",
                table: "FillingWarehouse",
                column: "MaterialId",
                principalTable: "Material",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

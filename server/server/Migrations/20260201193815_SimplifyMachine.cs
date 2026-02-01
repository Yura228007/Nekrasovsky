using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyMachine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machine_Warehouse_WarehouseId",
                table: "Machine");

            migrationBuilder.DropIndex(
                name: "IX_Machine_Code",
                table: "Machine");

            migrationBuilder.DropIndex(
                name: "IX_Machine_WarehouseId",
                table: "Machine");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Machine");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Machine");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Machine");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Machine");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Machine",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Machine",
                type: "varchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Machine",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Machine",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Machine",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Machine",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machine_Code",
                table: "Machine",
                column: "Code",
                unique: true,
                filter: "\"Code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Machine_WarehouseId",
                table: "Machine",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Machine_Warehouse_WarehouseId",
                table: "Machine",
                column: "WarehouseId",
                principalTable: "Warehouse",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class AddMachine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Machine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    Code = table.Column<string>(type: "varchar(50)", nullable: true),
                    Type = table.Column<string>(type: "varchar(100)", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Machine_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProductOutput",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    WorkReportId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: true),
                    MachineId = table.Column<int>(type: "integer", nullable: true),
                    ProducedQuantity = table.Column<int>(type: "integer", nullable: false),
                    DefectQuantity = table.Column<int>(type: "integer", nullable: false),
                    EcoQuantity = table.Column<int>(type: "integer", nullable: false),
                    MeasuringUnit = table.Column<string>(type: "varchar(50)", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOutput", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductOutput_Machine_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductOutput_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductOutput_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductOutput_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductOutput_WorkReport_WorkReportId",
                        column: x => x.WorkReportId,
                        principalTable: "WorkReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_MachineId",
                table: "ProductOutput",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_ProductId",
                table: "ProductOutput",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_UserId",
                table: "ProductOutput",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_WarehouseId",
                table: "ProductOutput",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_WorkReportId",
                table: "ProductOutput",
                column: "WorkReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductOutput");

            migrationBuilder.DropTable(
                name: "Machine");
        }
    }
}

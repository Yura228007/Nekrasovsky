using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class AddResponsibilityFilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResponsibilityFilling",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MeasuringUnit = table.Column<string>(type: "varchar(20)", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsibilityFilling", x => x.Id);
                    table.CheckConstraint("CK_ResponsibilityFilling_MaterialOrProduct", "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ResponsibilityFilling_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResponsibilityFilling_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResponsibilityFilling_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResponsibilityFilling_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityFilling_MaterialId",
                table: "ResponsibilityFilling",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityFilling_ProductId",
                table: "ResponsibilityFilling",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityFilling_UserId_WarehouseId_MaterialId",
                table: "ResponsibilityFilling",
                columns: new[] { "UserId", "WarehouseId", "MaterialId" },
                filter: "\"IsActive\" = true AND \"MaterialId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityFilling_UserId_WarehouseId_ProductId",
                table: "ResponsibilityFilling",
                columns: new[] { "UserId", "WarehouseId", "ProductId" },
                filter: "\"IsActive\" = true AND \"ProductId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityFilling_WarehouseId",
                table: "ResponsibilityFilling",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResponsibilityFilling");
        }
    }
}

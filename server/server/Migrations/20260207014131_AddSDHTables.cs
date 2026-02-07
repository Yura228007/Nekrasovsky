using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class AddSDHTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialSDHSale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<double>(type: "double precision", nullable: false),
                    MeasuringUnit = table.Column<string>(type: "varchar(20)", nullable: true),
                    SoldAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialSDHSale", x => x.Id);
                    table.CheckConstraint("CK_MaterialSDHSale_MaterialOrProduct", "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_MaterialSDHSale_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialSDHSale_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialSDHSale_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialSDHSale_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SDHRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromUserId = table.Column<int>(type: "integer", nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "integer", nullable: true),
                    FromWarehouseId = table.Column<int>(type: "integer", nullable: false),
                    ToWarehouseId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<double>(type: "double precision", nullable: false),
                    MeasuringUnit = table.Column<string>(type: "varchar(20)", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SDHRequest", x => x.Id);
                    table.CheckConstraint("CK_SDHRequest_MaterialOrProduct", "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_SDHRequest_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SDHRequest_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SDHRequest_User_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SDHRequest_User_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SDHRequest_Warehouse_FromWarehouseId",
                        column: x => x.FromWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SDHRequest_Warehouse_ToWarehouseId",
                        column: x => x.ToWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialSDHSale_MaterialId",
                table: "MaterialSDHSale",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialSDHSale_ProductId",
                table: "MaterialSDHSale",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialSDHSale_UserId",
                table: "MaterialSDHSale",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialSDHSale_WarehouseId",
                table: "MaterialSDHSale",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_SDHRequest_ApprovedByUserId",
                table: "SDHRequest",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SDHRequest_FromUserId",
                table: "SDHRequest",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SDHRequest_FromWarehouseId",
                table: "SDHRequest",
                column: "FromWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_SDHRequest_MaterialId",
                table: "SDHRequest",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_SDHRequest_ProductId",
                table: "SDHRequest",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SDHRequest_ToWarehouseId",
                table: "SDHRequest",
                column: "ToWarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialSDHSale");

            migrationBuilder.DropTable(
                name: "SDHRequest");
        }
    }
}

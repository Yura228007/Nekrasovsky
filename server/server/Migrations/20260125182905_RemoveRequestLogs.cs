using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRequestLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The table may already be absent in some databases (manual cleanup / drift).
            // Make the migration idempotent for PostgreSQL.
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""RequestLogs"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaterialId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    WarehouseId = table.Column<int>(type: "integer", nullable: true),
                    Action = table.Column<string>(type: "varchar(100)", nullable: true),
                    ClientIp = table.Column<string>(type: "varchar(50)", nullable: true),
                    Controller = table.Column<string>(type: "varchar(100)", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    HttpMethod = table.Column<string>(type: "varchar(10)", nullable: false),
                    RequestBody = table.Column<string>(type: "text", nullable: true),
                    RequestTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResponseBody = table.Column<string>(type: "text", nullable: true),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "varchar(500)", nullable: false),
                    UserAgent = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestLogs_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RequestLogs_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RequestLogs_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RequestLogs_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_MaterialId",
                table: "RequestLogs",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_ProductId",
                table: "RequestLogs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_UserId",
                table: "RequestLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_WarehouseId",
                table: "RequestLogs",
                column: "WarehouseId");
        }
    }
}

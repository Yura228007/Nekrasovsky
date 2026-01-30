using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using server.Data;

#nullable disable

namespace server.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260130140000_AddReprocessingSources")]
    public partial class AddReprocessingSources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReprocessingSourceItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReprocessingId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MeasuringType = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReprocessingSourceItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReprocessingSourceItem_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReprocessingSourceItem_Reprocessing_ReprocessingId",
                        column: x => x.ReprocessingId,
                        principalTable: "Reprocessing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReprocessingSourceItem_MaterialId",
                table: "ReprocessingSourceItem",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ReprocessingSourceItem_ReprocessingId",
                table: "ReprocessingSourceItem",
                column: "ReprocessingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReprocessingSourceItem");
        }
    }
}

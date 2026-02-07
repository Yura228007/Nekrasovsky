using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineIdToReprocessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MachineId",
                table: "Reprocessing",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reprocessing_MachineId",
                table: "Reprocessing",
                column: "MachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reprocessing_Machine_MachineId",
                table: "Reprocessing",
                column: "MachineId",
                principalTable: "Machine",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reprocessing_Machine_MachineId",
                table: "Reprocessing");

            migrationBuilder.DropIndex(
                name: "IX_Reprocessing_MachineId",
                table: "Reprocessing");

            migrationBuilder.DropColumn(
                name: "MachineId",
                table: "Reprocessing");
        }
    }
}

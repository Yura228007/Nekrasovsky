using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class ShiftReportFileContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: add column / alter only if not present (fixes broken migration history)
            migrationBuilder.Sql(@"
                ALTER TABLE ""ShiftReport"" ADD COLUMN IF NOT EXISTS ""FileContent"" bytea NULL;
                ALTER TABLE ""ShiftReport"" ALTER COLUMN ""FilePath"" DROP NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "ShiftReport");

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "ShiftReport",
                type: "varchar(500)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldNullable: true);
        }
    }
}

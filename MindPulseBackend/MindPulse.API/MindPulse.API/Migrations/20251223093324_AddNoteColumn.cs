using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindPulse.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "DailyLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "DailyLogs");
        }
    }
}

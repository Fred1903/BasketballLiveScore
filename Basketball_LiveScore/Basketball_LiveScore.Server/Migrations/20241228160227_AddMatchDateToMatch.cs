using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Basketball_LiveScore.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchDateToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "matchDate",
                table: "Match",
                newName: "MatchDate");

            migrationBuilder.AddColumn<int>(
                name: "Amount",
                table: "Timeout",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Timeout");

            migrationBuilder.RenameColumn(
                name: "MatchDate",
                table: "Match",
                newName: "matchDate");
        }
    }
}

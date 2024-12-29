using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Basketball_LiveScore.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeoutsToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AwayTeamRemainingTimeouts",
                table: "Match",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HomeTeamRemainingTimeouts",
                table: "Match",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayTeamRemainingTimeouts",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "HomeTeamRemainingTimeouts",
                table: "Match");
        }
    }
}

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Basketball_LiveScore.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddOnFieldIdsToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<int>>(
                name: "AwayTeamOnFieldIds",
                table: "Match",
                type: "integer[]",
                nullable: false,
                defaultValue: new List<int>());

            migrationBuilder.AddColumn<List<int>>(
                name: "HomeTeamOnFieldIds",
                table: "Match",
                type: "integer[]",
                nullable: false,
                defaultValue: new List<int>());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayTeamOnFieldIds",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "HomeTeamOnFieldIds",
                table: "Match");
        }
    }
}

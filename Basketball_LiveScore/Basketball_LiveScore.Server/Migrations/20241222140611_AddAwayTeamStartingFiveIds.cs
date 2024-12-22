using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Basketball_LiveScore.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAwayTeamStartingFiveIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<int>>(
                name: "AwayTeamStartingFiveIds",
                table: "Match",
                type: "integer[]",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "EncoderRealTimeId",
                table: "Match",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EncoderSettingsId",
                table: "Match",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<List<int>>(
                name: "HomeTeamStartingFiveIds",
                table: "Match",
                type: "integer[]",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "Quarter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MatchId = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    RemainingTime = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quarter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quarter_Match_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Match",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Timeout",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MatchId = table.Column<int>(type: "integer", nullable: false),
                    QuarterNumber = table.Column<int>(type: "integer", nullable: false),
                    TimeStamp = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Team = table.Column<string>(type: "text", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timeout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Timeout_Match_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Match",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quarter_MatchId",
                table: "Quarter",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Timeout_MatchId",
                table: "Timeout",
                column: "MatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quarter");

            migrationBuilder.DropTable(
                name: "Timeout");

            migrationBuilder.DropColumn(
                name: "AwayTeamStartingFiveIds",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "EncoderRealTimeId",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "EncoderSettingsId",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "HomeTeamStartingFiveIds",
                table: "Match");
        }
    }
}

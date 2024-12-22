using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Basketball_LiveScore.Server.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureMatchEventTPH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlayerId",
                table: "MatchEvent",
                newName: "FoulEvent_PlayerId");

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "MatchEvent",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "BasketEvent_PlayerId",
                table: "MatchEvent",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BasketEvent_Points",
                table: "MatchEvent",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FoulEvent_FoulType",
                table: "MatchEvent",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRunning",
                table: "MatchEvent",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayerInId",
                table: "MatchEvent",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayerOutId",
                table: "MatchEvent",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Team",
                table: "MatchEvent",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasketEvent_PlayerId",
                table: "MatchEvent");

            migrationBuilder.DropColumn(
                name: "BasketEvent_Points",
                table: "MatchEvent");

            migrationBuilder.DropColumn(
                name: "FoulEvent_FoulType",
                table: "MatchEvent");

            migrationBuilder.DropColumn(
                name: "IsRunning",
                table: "MatchEvent");

            migrationBuilder.DropColumn(
                name: "PlayerInId",
                table: "MatchEvent");

            migrationBuilder.DropColumn(
                name: "PlayerOutId",
                table: "MatchEvent");

            migrationBuilder.DropColumn(
                name: "Team",
                table: "MatchEvent");

            migrationBuilder.RenameColumn(
                name: "FoulEvent_PlayerId",
                table: "MatchEvent",
                newName: "PlayerId");

            migrationBuilder.AlterColumn<int>(
                name: "EventType",
                table: "MatchEvent",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13);
        }
    }
}

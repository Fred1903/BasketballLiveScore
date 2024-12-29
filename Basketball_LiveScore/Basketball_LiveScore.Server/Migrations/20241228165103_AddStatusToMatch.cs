using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Basketball_LiveScore.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Match",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Match");
        }
    }
}

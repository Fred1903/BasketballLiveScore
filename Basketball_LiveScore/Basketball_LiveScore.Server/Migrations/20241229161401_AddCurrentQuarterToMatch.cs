using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Basketball_LiveScore.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentQuarterToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentQuarter",
                table: "Match",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentQuarter",
                table: "Match");
        }
    }
}

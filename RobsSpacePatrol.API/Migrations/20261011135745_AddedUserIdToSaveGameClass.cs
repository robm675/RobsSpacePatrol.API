using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobsSpacePatrol.API.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserIdToSaveGameClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "SaveGames",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SaveGames");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobsSpacePatrol.API.Migrations
{
    /// <inheritdoc />
    public partial class AddedVersionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "SaveGames",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "SaveGames");
        }
    }
}

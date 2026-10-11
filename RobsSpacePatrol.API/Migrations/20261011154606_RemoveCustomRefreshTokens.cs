using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobsSpacePatrol.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCustomRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpiresDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    JwtTokenId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshTokenValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                });
        }
    }
}

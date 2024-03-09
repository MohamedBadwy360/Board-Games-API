using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGamesAPI.Migrations
{
    public partial class UpdateInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastModifiedTime",
                table: "Domains",
                newName: "LastModifiedDate");

            migrationBuilder.RenameColumn(
                name: "UserRatings",
                table: "BoardGames",
                newName: "UsersRated");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastModifiedDate",
                table: "Domains",
                newName: "LastModifiedTime");

            migrationBuilder.RenameColumn(
                name: "UsersRated",
                table: "BoardGames",
                newName: "UserRatings");
        }
    }
}

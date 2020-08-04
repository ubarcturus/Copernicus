using Microsoft.EntityFrameworkCore.Migrations;

namespace Copernicus_Weather.Migrations
{
    public partial class LocalHdUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserApod_AspNetUsers_IdentityUserId",
                table: "UserApod");

            migrationBuilder.AddColumn<string>(
                name: "LocalHdUrl",
                table: "Apod",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalHdUrl",
                table: "Apod");

            migrationBuilder.AddForeignKey(
                name: "FK_UserApod_AspNetUsers_IdentityUserId",
                table: "UserApod",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

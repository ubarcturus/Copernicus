#region

using Microsoft.EntityFrameworkCore.Migrations;

#endregion

namespace Copernicus_Weather.Migrations
{
    public partial class LocalHdUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                "FK_UserApod_AspNetUsers_IdentityUserId",
                "UserApod");

            migrationBuilder.AddColumn<string>(
                "LocalHdUrl",
                "Apod",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "LocalHdUrl",
                "Apod");

            migrationBuilder.AddForeignKey(
                "FK_UserApod_AspNetUsers_IdentityUserId",
                "UserApod",
                "IdentityUserId",
                "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
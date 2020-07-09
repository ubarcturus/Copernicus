using Microsoft.EntityFrameworkCore.Migrations;

namespace Copernicus_Weather.Migrations
{
    public partial class UserApod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "UserApod",
                table => new
                {
                    IdentityUserId = table.Column<string>(nullable: false),
                    ApodId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApod", x => new {x.IdentityUserId, x.ApodId});
                    table.ForeignKey(
                        "FK_UserApod_Apod_ApodId",
                        x => x.ApodId,
                        "Apod",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_UserApod_AspNetUsers_IdentityUserId",
                        x => x.IdentityUserId,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_UserApod_ApodId",
                "UserApod",
                "ApodId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "UserApod");
        }
    }
}
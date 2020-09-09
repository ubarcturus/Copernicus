using Microsoft.EntityFrameworkCore.Migrations;

namespace Copernicus_Weather.Migrations
{
    public partial class LocalSdUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalUrl",
                table: "Apod");

            migrationBuilder.AddColumn<string>(
                name: "LocalSdUrl",
                table: "Apod",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalSdUrl",
                table: "Apod");

            migrationBuilder.AddColumn<string>(
                name: "LocalUrl",
                table: "Apod",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

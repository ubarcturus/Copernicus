using Microsoft.EntityFrameworkCore.Migrations;

namespace Copernicus_Weather.Migrations
{
    public partial class LocalSdUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "LocalUrl",
                "Apod");

            migrationBuilder.AddColumn<string>(
                "LocalSdUrl",
                "Apod",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "LocalSdUrl",
                "Apod");

            migrationBuilder.AddColumn<string>(
                "LocalUrl",
                "Apod",
                "nvarchar(max)",
                nullable: true);
        }
    }
}
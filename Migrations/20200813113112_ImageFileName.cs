using Microsoft.EntityFrameworkCore.Migrations;

namespace Copernicus_Weather.Migrations
{
    public partial class ImageFileName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "Apod",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "Apod");
        }
    }
}

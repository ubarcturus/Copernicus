#region

using Microsoft.EntityFrameworkCore.Migrations;

#endregion

namespace Copernicus_Weather.Migrations
{
    public partial class ImageFileName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                "ImageFileName",
                "Apod",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "ImageFileName",
                "Apod");
        }
    }
}
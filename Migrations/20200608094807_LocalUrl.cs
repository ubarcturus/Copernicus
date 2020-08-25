#region

using Microsoft.EntityFrameworkCore.Migrations;

#endregion

namespace Copernicus_Weather.Migrations
{
    public partial class LocalUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                "LocalUrl",
                "Apod",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "LocalUrl",
                "Apod");
        }
    }
}
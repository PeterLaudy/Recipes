using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recepten.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIconIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconPath",
                table: "categorie");

            migrationBuilder.AddColumn<int>(
                name: "IconIndex",
                table: "categorie",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconIndex",
                table: "categorie");

            migrationBuilder.AddColumn<string>(
                name: "IconPath",
                table: "categorie",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recepten.Migrations
{
    /// <inheritdoc />
    public partial class AddCategorieIcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_gerecht_categorie_CategorieID",
                table: "gerecht");

            migrationBuilder.DropIndex(
                name: "IX_gerecht_CategorieID",
                table: "gerecht");

            migrationBuilder.DropColumn(
                name: "CategorieID",
                table: "gerecht");

            migrationBuilder.AddColumn<string>(
                name: "IconPath",
                table: "categorie",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "gerechtcategoriecombinatie",
                columns: table => new
                {
                    GerechtCategorieCombinatieID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GerechtID = table.Column<int>(type: "INTEGER", nullable: false),
                    CategorieID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gerechtcategoriecombinatie", x => x.GerechtCategorieCombinatieID);
                    table.ForeignKey(
                        name: "FK_gerechtcategoriecombinatie_categorie_CategorieID",
                        column: x => x.CategorieID,
                        principalTable: "categorie",
                        principalColumn: "CategorieID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_gerechtcategoriecombinatie_gerecht_GerechtID",
                        column: x => x.GerechtID,
                        principalTable: "gerecht",
                        principalColumn: "GerechtID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gerechtcategoriecombinatie_CategorieID",
                table: "gerechtcategoriecombinatie",
                column: "CategorieID");

            migrationBuilder.CreateIndex(
                name: "IX_gerechtcategoriecombinatie_GerechtID",
                table: "gerechtcategoriecombinatie",
                column: "GerechtID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gerechtcategoriecombinatie");

            migrationBuilder.DropColumn(
                name: "IconPath",
                table: "categorie");

            migrationBuilder.AddColumn<int>(
                name: "CategorieID",
                table: "gerecht",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_gerecht_CategorieID",
                table: "gerecht",
                column: "CategorieID");

            migrationBuilder.AddForeignKey(
                name: "FK_gerecht_categorie_CategorieID",
                table: "gerecht",
                column: "CategorieID",
                principalTable: "categorie",
                principalColumn: "CategorieID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

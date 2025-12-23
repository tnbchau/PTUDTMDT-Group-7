using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YenMay_web.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryArticleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryArticleId",
                table: "Articles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CategoryArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryArticles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_CategoryArticleId",
                table: "Articles",
                column: "CategoryArticleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_CategoryArticles_CategoryArticleId",
                table: "Articles",
                column: "CategoryArticleId",
                principalTable: "CategoryArticles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_CategoryArticles_CategoryArticleId",
                table: "Articles");

            migrationBuilder.DropTable(
                name: "CategoryArticles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_CategoryArticleId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "CategoryArticleId",
                table: "Articles");
        }
    }
}

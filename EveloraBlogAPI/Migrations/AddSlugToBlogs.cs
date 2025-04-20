using Microsoft.EntityFrameworkCore.Migrations;

namespace EverolaBlogAPI.Migrations
{
    public partial class AddSlugToBlogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Blogs",
                type: "text",
                nullable: true);
                
            // Update existing blogs to have slugs
            migrationBuilder.Sql(@"
                UPDATE ""Blogs"" 
                SET ""Slug"" = LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(""Title"", ' ', '-'), '&', 'and'), '?', ''), '!', ''), '.', ''), ',', ''), ':', ''), ';', ''), '(', ''), ')', ''))
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Blogs");
        }
    }
}

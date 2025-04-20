using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EverolaBlogAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Blogs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Blogs");

            migrationBuilder.InsertData(
                table: "Blogs",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "ImageUrl", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Software", "Security testing is a crucial aspect of software development...", new DateTime(2023, 4, 7, 0, 0, 0, 0, DateTimeKind.Utc), "/images/blogs/security.jpeg", "How To Tackle Security Testing And Challenges", null },
                    { 2, "Software", "Agile methodology has revolutionized the way we approach testing...", new DateTime(2022, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), "/images/blogs/agile.png", "Agile Testing: It's a new age of testing", null }
                });
        }
    }
}

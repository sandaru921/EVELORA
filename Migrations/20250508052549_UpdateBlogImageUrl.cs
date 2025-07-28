using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AssessmentPlatform.Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBlogImageUrl : Migration
    {
        /// <inheritdoc />
        // protected override void Up(MigrationBuilder migrationBuilder)
        // {
        //     migrationBuilder.CreateTable(
        //         name: "Blogs",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "integer", nullable: false)
        //                 .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
        //             Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
        //             Content = table.Column<string>(type: "text", nullable: false),
        //             Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
        //             ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
        //             Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
        //             CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
        //             UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_Blogs", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "Jobs",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "integer", nullable: false)
        //                 .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
        //             Title = table.Column<string>(type: "text", nullable: false),
        //             JobType = table.Column<string>(type: "text", nullable: false),
        //             Description = table.Column<string>(type: "text", nullable: false),
        //             ImageUrl = table.Column<string>(type: "text", nullable: false),
        //             CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_Jobs", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "Messages",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "integer", nullable: false)
        //                 .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
        //             Text = table.Column<string>(type: "text", nullable: false),
        //             Sender = table.Column<string>(type: "text", nullable: false),
        //             Recipient = table.Column<string>(type: "text", nullable: false),
        //             Timestamp = table.Column<string>(type: "text", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_Messages", x => x.Id);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "Users",
        //         columns: table => new
        //         {
        //             Id = table.Column<int>(type: "integer", nullable: false)
        //                 .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
        //             Username = table.Column<string>(type: "text", nullable: false),
        //             Email = table.Column<string>(type: "text", nullable: false),
        //             HashPassword = table.Column<string>(type: "text", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_Users", x => x.Id);
        //         });
        // }

        // /// <inheritdoc />
        // protected override void Down(MigrationBuilder migrationBuilder)
        // {
        //     migrationBuilder.DropTable(
        //         name: "Blogs");

        //     migrationBuilder.DropTable(
        //         name: "Jobs");

        //     migrationBuilder.DropTable(
        //         name: "Messages");

        //     migrationBuilder.DropTable(
        //         name: "Users");
        // }
        protected override void Up(MigrationBuilder migrationBuilder)
{
    // Don't try to create the table, just modify the column
    migrationBuilder.AlterColumn<string>(
        name: "ImageUrl",
        table: "Blogs",
        type: "character varying(500)",
        maxLength: 500,
        nullable: true,
        oldType: "character varying",
        oldNullable: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AlterColumn<string>(
        name: "ImageUrl",
        table: "Blogs",
        type: "character varying",
        nullable: true,
        oldType: "character varying(500)",
        oldMaxLength: 500,
        oldNullable: true);
}
    }
}

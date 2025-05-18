using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AssessmentPlatform.Backend.Migrations
{
    /// <inheritdoc />
    public partial class addQuizesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizName = table.Column<string>(type: "text", nullable: false),
                    QuizDuration = table.Column<int>(type: "integer", nullable: false),
                    QuizDate = table.Column<DateOnly>(type: "date", nullable: false),
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    JobsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quizes_Jobs_JobsId",
                        column: x => x.JobsId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quizes_JobsId",
                table: "Quizes",
                column: "JobsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quizes");
        }
    }
}

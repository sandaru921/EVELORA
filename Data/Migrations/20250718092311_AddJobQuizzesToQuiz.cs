using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AssessmentPlatform.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJobQuizzesToQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Permissions_QuizId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Options_PermissionId",
                table: "UserPermissions");

            migrationBuilder.DropTable(
                name: "Option");

            migrationBuilder.DropIndex(
                name: "IX_Quiz_QuizName",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "JobCategory",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "QuizDuration",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "QuizLevel",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "QuizName",
                table: "Permissions");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Jobs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Jobs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    QuizName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    JobCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    QuizDuration = table.Column<int>(type: "integer", nullable: false),
                    QuizLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobQuizzes",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    QuizId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobQuizzes", x => new { x.JobId, x.QuizId });
                    table.ForeignKey(
                        name: "FK_JobQuizzes_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobQuizzes_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobQuizzes_QuizId",
                table: "JobQuizzes",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_QuizName",
                table: "Quizzes",
                column: "QuizName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Options_Questions_QuestionId",
                table: "Options",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Quizzes_QuizId",
                table: "Questions",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Permissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Options_Questions_QuestionId",
                table: "Options");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Quizzes_QuizId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Permissions_PermissionId",
                table: "UserPermissions");

            migrationBuilder.DropTable(
                name: "JobQuizzes");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Permissions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobCategory",
                table: "Permissions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "QuizDuration",
                table: "Permissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QuizLevel",
                table: "Permissions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuizName",
                table: "Permissions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Jobs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Jobs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Option",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_Option_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "JobCategory", "QuizDuration", "QuizLevel", "QuizName" },
                values: new object[] { null, null, 0, null, null });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "JobCategory", "QuizDuration", "QuizLevel", "QuizName" },
                values: new object[] { null, null, 0, null, null });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "JobCategory", "QuizDuration", "QuizLevel", "QuizName" },
                values: new object[] { null, null, 0, null, null });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "JobCategory", "QuizDuration", "QuizLevel", "QuizName" },
                values: new object[] { null, null, 0, null, null });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "JobCategory", "QuizDuration", "QuizLevel", "QuizName" },
                values: new object[] { null, null, 0, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_QuizName",
                table: "Permissions",
                column: "QuizName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Permissions_QuizId",
                table: "Questions",
                column: "QuizId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Options_PermissionId",
                table: "UserPermissions",
                column: "PermissionId",
                principalTable: "Options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

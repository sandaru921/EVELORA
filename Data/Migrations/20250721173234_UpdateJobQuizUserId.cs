using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AssessmentPlatform.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJobQuizUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobQuizzes_Jobs_JobId",
                table: "JobQuizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_JobQuizzes_Quizzes_QuizId",
                table: "JobQuizzes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobQuizzes",
                table: "JobQuizzes");

            migrationBuilder.RenameTable(
                name: "JobQuizzes",
                newName: "JobQuiz");

            migrationBuilder.RenameIndex(
                name: "IX_JobQuizzes_QuizId",
                table: "JobQuiz",
                newName: "IX_JobQuiz_QuizId");

            migrationBuilder.AlterColumn<string>(
                name: "WorkMode",
                table: "Jobs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TechnicalSkills",
                table: "Jobs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SoftSkills",
                table: "Jobs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KeyResponsibilities",
                table: "Jobs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Jobs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiringDate",
                table: "Jobs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Experience",
                table: "Jobs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EducationalBackground",
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

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Jobs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Jobs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "JobQuiz",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "JobQuiz",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "JobQuiz",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "JobQuiz",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobQuiz",
                table: "JobQuiz",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_JobQuiz_JobId_QuizId",
                table: "JobQuiz",
                columns: new[] { "JobId", "QuizId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobQuiz_UserId",
                table: "JobQuiz",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobQuiz_UserId1",
                table: "JobQuiz",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_JobQuiz_Jobs_JobId",
                table: "JobQuiz",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobQuiz_Quizzes_QuizId",
                table: "JobQuiz",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobQuiz_Users_UserId",
                table: "JobQuiz",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JobQuiz_Users_UserId1",
                table: "JobQuiz",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobQuiz_Jobs_JobId",
                table: "JobQuiz");

            migrationBuilder.DropForeignKey(
                name: "FK_JobQuiz_Quizzes_QuizId",
                table: "JobQuiz");

            migrationBuilder.DropForeignKey(
                name: "FK_JobQuiz_Users_UserId",
                table: "JobQuiz");

            migrationBuilder.DropForeignKey(
                name: "FK_JobQuiz_Users_UserId1",
                table: "JobQuiz");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobQuiz",
                table: "JobQuiz");

            migrationBuilder.DropIndex(
                name: "IX_JobQuiz_JobId_QuizId",
                table: "JobQuiz");

            migrationBuilder.DropIndex(
                name: "IX_JobQuiz_UserId",
                table: "JobQuiz");

            migrationBuilder.DropIndex(
                name: "IX_JobQuiz_UserId1",
                table: "JobQuiz");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "JobQuiz");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "JobQuiz");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "JobQuiz");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "JobQuiz");

            migrationBuilder.RenameTable(
                name: "JobQuiz",
                newName: "JobQuizzes");

            migrationBuilder.RenameIndex(
                name: "IX_JobQuiz_QuizId",
                table: "JobQuizzes",
                newName: "IX_JobQuizzes_QuizId");

            migrationBuilder.AlterColumn<string>(
                name: "WorkMode",
                table: "Jobs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "TechnicalSkills",
                table: "Jobs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SoftSkills",
                table: "Jobs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "KeyResponsibilities",
                table: "Jobs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Jobs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiringDate",
                table: "Jobs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "Experience",
                table: "Jobs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EducationalBackground",
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

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Jobs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Jobs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobQuizzes",
                table: "JobQuizzes",
                columns: new[] { "JobId", "QuizId" });

            migrationBuilder.AddForeignKey(
                name: "FK_JobQuizzes_Jobs_JobId",
                table: "JobQuizzes",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobQuizzes_Quizzes_QuizId",
                table: "JobQuizzes",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
